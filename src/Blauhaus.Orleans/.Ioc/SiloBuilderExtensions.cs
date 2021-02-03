using System;
using System.Net;
using System.Reflection;
using Blauhaus.Common.ValueObjects.BuildConfigs;
using Blauhaus.Orleans.Config;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;

namespace Blauhaus.Orleans.Ioc
{
    public static class SiloBuilderExtensions
    {
        public static ISiloBuilder AddOrleansDashboard(this ISiloBuilder siloBuilder, int port)
        {
            siloBuilder
                .UseDashboard(options =>
                { 
                    options.Host = "*";
                    options.Port = port;
                    options.HostSelf = true;
                    options.CounterUpdateIntervalMs = 10000;
                })
                .UseLinuxEnvironmentStatistics();

            return siloBuilder;
        }

        public static ISiloBuilder ConfigureSilo(this ISiloBuilder siloBuilder, IOrleansConfig clusterConfig, Assembly grainAssembly)
        {
            siloBuilder
                .UseDashboard(options =>
                { 
                    options.Host = "*";
                    options.Port = clusterConfig.DashboardPort;
                    options.HostSelf = true;
                    options.CounterUpdateIntervalMs = 10000;
                })
                .UseLinuxEnvironmentStatistics();

                siloBuilder
                .UseAzureStorageClustering(options =>
                {
                    options.ConnectionString = clusterConfig.AzureStorageConnectionString;
                    options.TableName = clusterConfig.StorageTableName;
                })
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddFromApplicationBaseDirectory(); 
                    parts.AddApplicationPart(grainAssembly).WithReferences();
                });


            if (clusterConfig.BuildConfig.Equals(BuildConfig.Debug))
            {
                siloBuilder.ConfigureEndpoints(Dns.GetHostName(), 11111, 30000);
                siloBuilder.Configure<ClusterMembershipOptions>(x => x.ValidateInitialConnectivity = false);
                siloBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ServiceId = clusterConfig.ServiceId;
                    options.ClusterId = clusterConfig.ClusterId;
                });
            }
            else
            {
                siloBuilder.UseKubernetesHosting();
            }

            return siloBuilder;
        }
    }
}