using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _Key;
        public TokenService(IConfiguration config)
        {
           _Key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            var claims=new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString() ),
                new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName )
            };
            var creds=new SigningCredentials(_Key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor=new SecurityTokenDescriptor(){
                Subject=new ClaimsIdentity(claims),
                Expires= DateTime.Now.AddDays(7),
                SigningCredentials=creds
            };

            var tokenHandler=new JwtSecurityTokenHandler(); //here start

            var token=tokenHandler.CreateJwtSecurityToken(tokenDescriptor);//here create token send all info client and needed
              
            return tokenHandler.WriteToken(token);//return written token to whowever needs it
        }
    }
}