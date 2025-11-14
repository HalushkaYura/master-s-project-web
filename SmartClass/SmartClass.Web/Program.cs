using Radzen;
using SmartClass.Application;
using SmartClass.Application.Abstractions;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Data.Repositories;
using SmartClass.Web.Components;
using SmartClass.Web.Services;

namespace SmartClass.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Application + Infrastructure
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();

            // ----------  GLOBAL HTTPCLIENT  ----------
            builder.Services.AddScoped(sp =>
            {
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext
                                   ?? throw new InvalidOperationException("No HttpContext");

                var request = httpContext.Request;
                var baseUri = $"{request.Scheme}://{request.Host}";

                return new HttpClient
                {
                    BaseAddress = new Uri(baseUri)
                };
            });

            // ----------  AUTH API CLIENT ----------
            builder.Services.AddScoped<AuthApiClient>();

            // ----------  LOCAL STORAGE ----------
            builder.Services.AddScoped<LocalStorage>();

            // ----------  CURRENT USER ----------
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            // ----------  GENERAL REPOSITORY ----------
            builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            // ----------  RADZEN ----------
            builder.Services.AddRadzenComponents();

            // ---------- API + SWAGGER ----------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // API Controllers
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

            // Map API
            app.MapControllers();

            // Map Blazor
            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
