using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ECARTemplate.Data;
using ECARTemplate.Models;
using ECARTemplate.Extensions;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace ECARTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SmartSettings>(Configuration.GetSection(SmartSettings.SectionName));
            services.AddSingleton(s => s.GetRequiredService<IOptions<SmartSettings>>().Value);

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // --- INICIO: CAMBIOS PARA AUTENTICACIÓN POR DIRECTORIO ACTIVO ---

            // Comentamos la configuración de autenticación por cookies personalizada
            // services.AddAuthentication("Custom")
            //     .AddCookie("Custom", options =>
            //     {
            //         options.LoginPath = "/Page/Login";
            //         options.LogoutPath = "/Page/Logout";
            //         options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            //         options.SlidingExpiration = true;
            //         options.Events.OnRedirectToAccessDenied = context =>
            //         {
            //             context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //             context.Response.ContentType = "text/html; charset=utf-8";
            //             string script = "<script>alert('¡No autorizado!'); window.location.href = '/';</script>";
            //             return context.Response.WriteAsync(script);
            //         };
            //     });

            // Agregamos la autenticación de Negociación para el Directorio Activo
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            // --- FIN: CAMBIOS PARA AUTENTICACIÓN POR DIRECTORIO ACTIVO ---

            services.AddAuthorization(options =>
            {
                // Estas políticas de autorización siguen siendo válidas para el nuevo esquema de autenticación
                options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy => policy.RequireRole("Estandar"));
            });

            // Comentamos el ClaimsTransformer ya que la lógica de roles podría cambiar
            // services.AddSingleton<IClaimsTransformation, ClaimsTransformer>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Ahora que el login se maneja por Windows, podemos cambiar la ruta por defecto
                // para que vaya directamente al Dashboard o a una página principal.
                // endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Page}/{action=Login}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "Admin",
                    pattern: "{controller=Admin}/{action=Index}");

                endpoints.MapControllerRoute(
                    name: "empleados",
                    pattern: "{controller=Empleados}/{action=Details}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}