using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using SmartClass.Application;
using SmartClass.Application.Abstractions;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Data.Repositories;
using SmartClass.Infrastructure.Options;
using SmartClass.Web.Auth;
using SmartClass.Web.Components;
using SmartClass.Web.Hubs;
using SmartClass.Web.Services;
using System.Text;
namespace SmartClass.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddRazorPages();

            builder.Services.AddHttpContextAccessor();

            //Services
            builder.Services.AddScoped<ICurrentUserService, CurrentUser>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            builder.Services.AddScoped<DevJwtState>();

            builder.Services.AddHttpClient();

            // SignalR + UserIdProvider
            builder.Services.AddSignalR().AddJsonProtocol();
            builder.Services.AddSingleton<IUserIdProvider, GuidUserIdProvider>();
            builder.Services.AddScoped<IChatRealtimeSender, ChatRealtimeSender>();

            // Реєструємо реалізацію адаптера
            builder.Services.AddScoped<INotificationRealtimeSender, NotificationRealtimeSender>();
            // JWT + Hub підтримка
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ValidateLifetime = true
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "SmartClass API", Version = "v1" });

                // JWT Bearer у Swagger
                options.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Введи токен як: Bearer {your JWT}"
                });

                options.AddSecurityRequirement(new()
                {
                        {
                            new() {
                                Reference = new() {
                                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                });
            });

            // Controllers
            builder.Services.AddControllers();
            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllers();

            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();


            app.MapRazorPages();
            app.MapHub<NotificationsHub>("/hubs/notifications"); // endpoint для клієнтів

            app.MapHub<ChatHub>("/hubs/chat");


            app.Run();

        }
    }
}
