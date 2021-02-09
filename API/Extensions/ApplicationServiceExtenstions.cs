using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using API.Helpers;
using API.SignalR;

namespace API.Extensions
{
    public static class ApplicationServiceExtenstions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services ,IConfiguration config){

             services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings")); 
            services.AddScoped<ITokenService,TokenService>();//jwt  
             services.AddScoped<IPhotoService,PhotoService>();//photoservice cloudinary  
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            // services.AddScoped<ILikesRepository,LikesRepository>();//likes   
            // services.AddScoped<IMessagesRepository,MessagesRepository>();//Messages   
           services.AddScoped<LogUserActivity>();// last active 
           //services.AddScoped<IUserRepository,UserRepository>();//user repository 
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