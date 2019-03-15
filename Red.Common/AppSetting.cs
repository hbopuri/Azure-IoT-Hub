using System;
using Microsoft.Extensions.Configuration;

namespace Red.Common
{
    public static class AppSetting
    {
        public static IConfigurationRoot Load(string directory)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appSettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}