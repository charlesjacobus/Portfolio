using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Portfolio.Api.Builders;
using Portfolio.Business.Services;
using Portfolio.Business.Services.Health;
using Microsoft.AspNetCore.DataProtection;

namespace Portfolio.Api
{
    public class Startup
    {
        const string AllowCrossProjectOrigins = "_allowCrossProjectOrigins";

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

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy(AllowCrossProjectOrigins,
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            services.AddScoped<IExhibitService, ExhibitService>();
            services.AddScoped<IHealthServices, HealthServices>();
            services.AddSingleton<PortfolioRepresentationBuilder, PortfolioRepresentationBuilder>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Portfolio API",
                    Description = "Charles Jacobus Selected Works",
                    TermsOfService = null,
                    Contact = new OpenApiContact
                    {
                        Name = "Charles Jacobus",
                        Email = "charleshenryjacobus@gmail.com",
                        Url = new Uri("https://www.instagram.com/charleshenryjacobus/")
                    }
                    //License = new OpenApiLicense
                    //
                    //    Name = "Use under LICX",
                    //    Url = new Uri("https://example.com/license"),
                    //}
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // app.UseAuthorization();

            app.UseCors(AllowCrossProjectOrigins);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
