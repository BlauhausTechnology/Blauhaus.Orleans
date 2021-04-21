using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Common.ValueObjects.BuildConfigs;
using Microsoft.Extensions.Configuration;

namespace Blauhaus.Orleans.Extensions
{
    public static class ConfigurationExtensions
    {
        
        public static IBuildConfig ExtractBuildConfig(this IConfiguration configuration)
        {
            var environment = configuration.GetSection("ASPNETCORE_ENVIRONMENT");

            if (environment.Value == null)
            {
                throw new InvalidOperationException("Could not find ASPNETCORE_ENVIRONMENT in Configuration");
            }

            if (environment.Value.ToLowerInvariant().Equals("development")) return BuildConfig.Debug;
            if (environment.Value.ToLowerInvariant().Equals("testing")) return BuildConfig.Test;
            if (environment.Value.ToLowerInvariant().Equals("staging")) return BuildConfig.Staging;
            if (environment.Value.ToLowerInvariant().Equals("production")) return BuildConfig.Release;

            throw new InvalidOperationException($"{environment.Value} was not recognized as a valid build config");
        }


        public static string ExtractClusterValue(this IConfiguration configuration, string key)
        {
            var clusterConfigSection = configuration.GetSection("Cluster");
            if (clusterConfigSection == null)
            {
                throw new InvalidOperationException("Could not find \'Cluster\' section in Configuration");
            }

            var configItems = clusterConfigSection.GetChildren().ToList();
            if (configItems == null || configItems.Count == 0)
            {
                throw new InvalidOperationException("\'Cluster\' section contained no information");
            }

            return configItems.ExtractValue(key);
        }

        
        private static string ExtractValue(this IEnumerable<IConfigurationSection> configItems, string key)
        {
            var configItem = configItems.FirstOrDefault(x => x.Key == key);
            if (configItem == null || string.IsNullOrEmpty(configItem.Value))
            {
                throw new InvalidOperationException("Configuration did not contain a value for " + key);
            }

            return configItem.Value;
        }

        public static IConfiguration PrintValues(this IConfiguration configuration)
        {
            foreach (var configurationSection in configuration.GetChildren())
            {
                Console.Out.WriteLine(configurationSection.Key + ": " + configurationSection.Value);
                foreach (var kid in configurationSection.GetChildren())
                {
                    Console.Out.WriteLine(">> "  +kid.Key + ": " + kid.Value);
                }
            }

            return configuration;
        }
    }
}