using System;
using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace Portfolio.Api.Representations
{
    public class ApplicationSettingsRepresentation
    {
        public string ApplicationName { get; set; }

        public string AssemblyVersion { get; set; }

        public string InformationalVersion { get; set; }

        public static ApplicationSettingsRepresentation Configure(IConfiguration configuration)
        {
            if (configuration == null)
            {
                return null;
            }

            var applicationSettings = new ApplicationSettingsRepresentation();

            configuration.GetSection("ApplicationSettings").Bind(applicationSettings);
            applicationSettings.AssemblyVersion = Convert.ToString(Assembly.GetEntryAssembly().GetName().Version);
            applicationSettings.InformationalVersion = Convert.ToString(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

            return applicationSettings;
        }
    }
}
