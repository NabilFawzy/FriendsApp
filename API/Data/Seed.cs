using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext dataContext){
            if(await dataContext.Users.AnyAsync())return;

            var userData=System.IO.File.ReadAllText("Data/UserSeedData.json");
            var users=JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach (var user in users)
            {
                using var hmac=new HMACSHA512();
                user.UserName=user.UserName.ToLower();
                 user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes("passw..d"));
                 user.PasswordSalt=hmac.Key;
                 dataContext.Users.Add(user);
            }
            await dataContext.SaveChangesAsync();
        }
    }
}