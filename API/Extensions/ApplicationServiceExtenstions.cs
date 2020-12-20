using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using API.Helpers;

namespace API.Extensions
{
    public static class ApplicationServiceExtenstions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services ,IConfiguration config){
            services.AddScoped<ITokenService,TokenService>();//jwt  
           services.AddScoped<IUserRepository,UserRepository>();//user repository 
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
             services.AddDbContext<DataContext>(
                 options=>{
                     options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
                 }
             );
             return services;
        }
    
    }
}