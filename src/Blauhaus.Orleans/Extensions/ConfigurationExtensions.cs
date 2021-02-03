using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Blauhaus.Orleans.Extensions
{
    public static class ConfigurationExtensions
    {
        
        public static string ExtractClusterValue(this IConfiguration configuration, string key)
        {
            var clusterConfigSection = configuration.GetSection("Cluster");
            if (clusterConfigSection == null)
            {
                throw new InvalidOperationException("Could not find \'Cluster\' section in Configuration");
            }

            List<IConfigurationSection>? configItems = clusterConfigSection.GetChildren().ToList();
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
    }
}