using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.OpenApi.Models;
using Radzen;
using SmartClass.Application;
using SmartClass.Application.Abstractions;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Data.Repositories;
using SmartClass.Infrastructure.Services.Auth;
using SmartClass.Web.Components;
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

            // ---------- COOKIE POLICY ----------
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.Always;

                options.OnAppendCookie = ctx =>
                {
                    // вс≥ кук≥ т≥льки по HTTPS
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
                    // https://localhost:44397 (або ≥нший хост/порт)
                    client.BaseAddress = new Uri($"{req.Scheme}://{req.Host}");
                }
                else
                {
                    // fallback на випадок виклик≥в без HttpContext (наприклад, фонов≥ задач≥)
                    client.BaseAddress = new Uri("https://localhost:44397");
                }
            });

            // ---------- LOCAL STORAGE / USER STATE ----------
            builder.Services.AddScoped<LocalStorage>();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<CurrentUserState>();

            // ---------- GENERAL REPOSITORY ----------
            builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            // ---------- RADZEN ----------
            builder.Services.AddRadzenComponents();
            builder.Services.AddRazorComponents()
                         .AddInteractiveServerComponents()
                         .AddCircuitOptions(options => options.DetailedErrors = true); // <--- ƒќƒј…“≈ ÷≈ ƒЋя ƒ≈Ѕј√”

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
                    Description = "¬вед≥ть JWT Token у формат≥: Bearer {токен}"
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
            builder.Services.AddScoped<IChatRealtimeSender, ChatRealtimeSender>();



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

            app.Run();
        }
    }
}
