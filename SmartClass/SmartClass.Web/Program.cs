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

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();

            //Services
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

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
                    Description = "¬веди токен €к: Bearer {your JWT}"
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

            app.Run();

        }
    }
}
