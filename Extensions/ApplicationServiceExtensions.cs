using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
        public static class ApplicationServiceExtensions
    {
      public static IServiceCollection AddApplicationServices(this IServiceCollection services,
      IConfiguration config)
      {
            services.AddCors();
            services.AddScoped<ITokenService,TokenService>();
            return services;
      }
    }
}