using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Options;
using SmartClass.Infrastructure.Helpers.Mapping;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Options;
using SmartClass.Infrastructure.Persistence;
using SmartClass.Infrastructure.Services;
using SmartClass.Infrastructure.Services.Auth;
using SmartClass.Infrastructure.Storage;
using System.Text;
using FileStorageOptions = SmartClass.Infrastructure.Storage.FileStorageOptions;

namespace SmartClass.Infrastructure;

public static class AddInfrastructureExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1) Звичайний DbContext (для Identity, якщо треба)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // 2) ФАБРИКА КОНТЕКСТУ – робимо її SCOPED, щоб не було конфлікту з options
        services.AddDbContextFactory<AppDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);



        services.Configure<FileStorageOptions>(
            configuration.GetSection("FileStorage"));

        services.AddSingleton<IFileStorage, LocalFileStorage>();

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
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // ГОЛОВНЕ: читаємо токен з cookie "accessToken"
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("accessToken", out var token))
                        {
                            ctx.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };

                // ДОДАТКОВО: читаємо токен з query ?access_token=... для файлів
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // пробуємо знайти токен в query ?access_token=...
                        var accessToken = context.Request.Query["access_token"];

                        // застосовуємо це тільки для файлів (щоб не ловити зайве)
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/api/files"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IGradebookService, GradebookService>();
        services.AddScoped<INotificationService, NotificationService>();


        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        //var fsOptions = configuration.GetSection("FileStorage").Get<FileStorageOptions>();
        //if (fsOptions.Provider == "Local")
        //{
        //    services.AddScoped<IFileStorage, LocalFileStorage>();
        //}

        services.AddAutoMapper(cfg => cfg.AddProfile<ApplicationProfile>());

        return services;
    }
}
