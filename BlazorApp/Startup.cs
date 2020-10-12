using BlazorApp.BLL.Hubs;
using BlazorApp.BLL.Infrastructure.Extensions;
using BlazorApp.BLL.Infrastructure.Helpers;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Infrastructure.Extensions;
using BlazorApp.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Reflection;

namespace BlazorApp
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBaseConfiguration(Configuration, _environment);
            services.AddConfiguration(Configuration, typeof(Startup).GetTypeInfo().Assembly);

            services.AddAppServicesConfiguration();

            services.AddManagersCollection();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCookiePolicy();

            EmailTemplates.Initialize(env, Configuration);

            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Debug.Print(msg);
                Debugger.Break();
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var databaseInitializer = serviceScope.ServiceProvider.GetService<IDatabaseInitializer>();
                databaseInitializer.SeedAsync().Wait();
            }

            app.UseMiddleware<ApiResponseRequestLoggingMiddleware>(
                Convert.ToBoolean(Configuration["BlazorApp:APILogging:Enabled"] ?? "true"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

#if ClientSideBlazor
                app.UseWebAssemblyDebugging();
#endif
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

#if ClientSideBlazor
            app.UseBlazorFrameworkFiles();
#endif
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseMiddleware<UserSessionMiddleware>();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chathub");

#if !ClientSideBlazor
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
#else
                endpoints.MapFallbackToFile("index.html");
#endif
            });
        }
    }
}