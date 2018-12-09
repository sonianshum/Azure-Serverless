using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UserActionsFunctionApp.DependencyInjection
{
    public static class BuildConfiguration
    {
        public static IConfigurationRoot Build()
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location
                .Split(new[] { @"\bin" }, StringSplitOptions.None)[0];

            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("settings.json");

            var configuration = builder.Build();
            return configuration;
        }
    }
}
