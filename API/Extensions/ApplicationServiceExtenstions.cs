using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ApplicationServiceExtenstions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services ,IConfiguration config){
            services.AddScoped<ITokenService,TokenService>();//jwt 
             services.AddDbContext<DataContext>(
                 options=>{
                     options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
                 }
             );
             return services;
        }
    
    }
}