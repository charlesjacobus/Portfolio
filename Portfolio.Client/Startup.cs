using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Portfolio.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var keyStorageFolderFullName = Configuration.GetValue<string>("ApplicationSettings:KeyStorageFolderFullName");
            if (!string.IsNullOrWhiteSpace(keyStorageFolderFullName) && Directory.Exists(keyStorageFolderFullName))
            {
                services
                    .AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keyStorageFolderFullName));
            }

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
                spa.Options.StartupTimeout = new TimeSpan(0, 5, 0);

                if (env.IsDevelopment())
                {
                    // In package.json, required adding "--verbose" switch to ng server in start script after upgrading to Angular 9
                    // Reference: https://medium.com/@YvonneFPan/how-to-integrate-angular-9-with-a-asp-net-core-project-cb37d9da7197
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
