﻿using Microsoft.Extensions.Configuration;
using System.IO;

namespace pyme_finance_api.Common
{
    public class ConfigHelper
    {

        private static ConfigHelper _appSettings;

        public string appSettingValue { get; set; }

        public static string AppSetting(string Key)
        {
            _appSettings = GetCurrentSettings(Key);
            return _appSettings.appSettingValue;
        }

        public ConfigHelper(IConfiguration config, string Key)
        {
            this.appSettingValue = config.GetValue<string>(Key);
        }

        public static ConfigHelper GetCurrentSettings(string Key)
        {

            var builder = new ConfigurationBuilder()

                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var settings = new ConfigHelper(configuration.GetSection("MyConfig"), Key);

            return settings;
        }
    }
}
