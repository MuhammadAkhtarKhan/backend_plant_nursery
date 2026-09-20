using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PlantNursery.Application.Authentication;
using PlantNursery.Application.Interfaces;
using PlantNursery.Infrastructure.Identity;
using PlantNursery.Infrastructure.Persistence;
using PlantNursery.Infrastructure.Services;

namespace PlantNursery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Data Protection
        services.AddDataProtection();

        // Identity
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // JWT
        var jwtSettings = configuration
            .GetSection("Jwt")
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JWT settings are missing.");

        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt"));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = JwtEvents.OnChallenge,
                    OnForbidden = JwtEvents.OnForbidden
                };
            });

        services.AddAuthorization();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}