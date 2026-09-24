using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Portfolio.Client
{
    public class Startup
    {
        // Must match architect.serve.options.port in ClientApp/angular.json
        private const int AngularDevServerPort = 4200;

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    var logger = loggerFactory.CreateLogger("AngularDevServer");
                    var angularProcess = StartAngularDevServer(spa.Options.SourcePath, AngularDevServerPort, logger);

                    appLifetime.ApplicationStopping.Register(() => StopAngularDevServer(angularProcess, logger));

                    spa.UseProxyToSpaDevelopmentServer($"http://localhost:{AngularDevServerPort}");
                }
            });
        }

        // Launches the Angular CLI dev server (via "npm start") directly and waits for its port to
        // accept connections, rather than relying on Microsoft.AspNetCore.SpaServices.AngularCli's
        // UseAngularCliServer, which detects readiness by pattern-matching webpack-dev-server's
        // console output. That doesn't work with Angular's esbuild/Vite-based dev server, which
        // this project now uses, so readiness is detected by polling the port instead.
        private static Process StartAngularDevServer(string sourcePath, int port, ILogger logger)
        {
            var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), sourcePath);

            var startInfo = OperatingSystem.IsWindows()
                ? new ProcessStartInfo("cmd", "/c npm start")
                : new ProcessStartInfo("npm", "start");

            startInfo.WorkingDirectory = workingDirectory;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            logger.LogInformation("Starting Angular CLI dev server (npm start) in {WorkingDirectory}...", workingDirectory);

            var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start the Angular CLI dev server process.");

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogInformation("[ng serve] {Line}", e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogInformation("[ng serve] {Line}", e.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            WaitForPortToAcceptConnections(port, TimeSpan.FromMinutes(5), process, logger);

            return process;
        }

        private static void WaitForPortToAcceptConnections(int port, TimeSpan timeout, Process process, ILogger logger)
        {
            var deadline = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < deadline)
            {
                if (process.HasExited)
                {
                    throw new InvalidOperationException(
                        $"The Angular CLI dev server process exited unexpectedly (exit code {process.ExitCode}) before it started listening on port {port}.");
                }

                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync("localhost", port);
                    if (connectTask.Wait(TimeSpan.FromMilliseconds(500)) && client.Connected)
                    {
                        logger.LogInformation("Angular CLI dev server is listening on port {Port}.", port);

                        return;
                    }
                }
                catch
                {
                    // Not listening yet; keep retrying until the deadline.
                }

                Thread.Sleep(500);
            }

            throw new TimeoutException(
                $"The Angular CLI dev server did not start listening on port {port} within {timeout.TotalSeconds} seconds. Check the log output above for error information.");
        }

        private static void StopAngularDevServer(Process process, ILogger logger)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to stop the Angular CLI dev server process.");
            }
        }
    }
}
