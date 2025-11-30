using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Options;
using SmartClass.Infrastructure.Files;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Identity.Services;
using SmartClass.Infrastructure.Mapping;
using SmartClass.Infrastructure.Notifications;
using SmartClass.Infrastructure.Options;
using SmartClass.Infrastructure.Persistence;
using System.Text;

namespace SmartClass.Infrastructure;

public static class AddInfrastructureExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        var fsOptions = configuration.GetSection("FileStorage").Get<FileStorageOptions>();
        if (fsOptions.Provider == "Local")
        {
            services.AddScoped<IFileStorage, LocalFileStorage>();
        }
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IFileStorage, LocalFileStorage>();
        
        services.AddScoped<INotificationService, NotificationService>();

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
                .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>()!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddAutoMapper(cfg => cfg.AddProfile<ApplicationProfile>());

        return services;
    }
}
