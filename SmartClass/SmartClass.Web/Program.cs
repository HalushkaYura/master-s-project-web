using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.OpenApi.Models;
using Radzen;
using SmartClass.Application;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Auth;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Data.Repositories;
using SmartClass.Infrastructure.Services.Auth;
using SmartClass.Web.Components;
using SmartClass.Web.Hubs;
using SmartClass.Web.Services;

namespace SmartClass.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------- APPLICATION + INFRASTRUCTURE ----------
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            // ---------- MVC + RAZOR/BLazor ----------
            builder.Services.AddControllers();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // ---------- HTTP CONTEXT ----------
            builder.Services.AddHttpContextAccessor();
            // ---------- DEFAULT HTTPCLIENT ДЛЯ ВНУТРІШНІХ API ВИКЛИКІВ ----------
            builder.Services.AddScoped(sp =>
            {
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

                if (httpContext != null)
                {
                    var req = httpContext.Request;
                    // Напр. https://localhost:44397
                    return new HttpClient
                    {
                        BaseAddress = new Uri($"{req.Scheme}://{req.Host}")
                    };
                }

                // fallback (на всякий випадок)
                return new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:44397")
                };
            });

            // ---------- COOKIE POLICY ----------
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.Always;

                options.OnAppendCookie = ctx =>
                {
                    // всі кукі тільки по HTTPS
                    ctx.CookieOptions.Secure = true;
                };
            });

            // ---------- AUTH API CLIENT (TYPED HTTPCLIENT) ----------
            builder.Services.AddHttpClient<AuthApiClient>((sp, client) =>
            {
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

                if (httpContext != null)
                {
                    var req = httpContext.Request;
                    // https://localhost:44397 (або інший хост/порт)
                    client.BaseAddress = new Uri($"{req.Scheme}://{req.Host}");
                }
                else
                {
                    // fallback на випадок викликів без HttpContext (наприклад, фонові задачі)
                    client.BaseAddress = new Uri("https://localhost:44397");
                }
            });

            // ---------- LOCAL STORAGE / USER STATE ----------
            builder.Services.AddScoped<LocalStorage>();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<CurrentUserState>();
            builder.Services.AddScoped<ITokenProvider, LocalStorageTokenProvider>();
            builder.Services.AddScoped<ILocalizationService, LocalizationService>();

            // ---------- GENERAL REPOSITORY ----------
            builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            // ---------- RADZEN ----------
            builder.Services.AddRadzenComponents(); // <-- ДОДАЙТЕ ЦЕЙ РЯДОК
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddCircuitOptions(options => options.DetailedErrors = true);
            // ...

            // ---------- SWAGGER ----------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SmartClass API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введіть JWT Token у форматі: Bearer {токен}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ---------- APPLICATION SERVICES ----------
            //builder.Services.AddScoped<IChatClient, Chat>();



            var app = builder.Build();

            // ---------- MIDDLEWARE PIPELINE ----------
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            // API
            app.MapControllers();

            // Blazor
            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            // Статичні файли з wwwroot (важливо для env.WebRootPath)
            app.UseStaticFiles();


            // ➕ ДОДАЄМО hub для чату
            app.MapHub<ChatHub>("/hubs/chat");
            app.Run();
        }
    }
}
