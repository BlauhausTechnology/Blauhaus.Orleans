using System;
using System.Net;
using System.Reflection;
using Blauhaus.Common.ValueObjects.BuildConfigs;
using Blauhaus.Orleans.Abstractions.Streams;
using Blauhaus.Orleans.Config;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;

namespace Blauhaus.Orleans.Ioc
{
    public static class SiloBuilderExtensions
    {

        public static ISiloBuilder ConfigureDashboard(this ISiloBuilder siloBuilder, int port)
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
        
        public static ISiloBuilder ConfigureClustering(this ISiloBuilder siloBuilder, IBuildConfig buildConfig, string storageConnectionString, string clusterName)
        {
            siloBuilder              
                .UseAzureStorageClustering(options =>
            {
                options.ConnectionString = storageConnectionString;
                options.TableName = clusterName + "ClusterInfo";
            });

            if (buildConfig.Equals(BuildConfig.Debug))
            {
                siloBuilder.ConfigureEndpoints(Dns.GetHostName(), 11111, 30000);
                siloBuilder.Configure<ClusterMembershipOptions>(x => x.ValidateInitialConnectivity = false);
                siloBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ServiceId = clusterName + "Service";
                    options.ClusterId = clusterName + "Cluster";
                });
            }
            else
            {
                siloBuilder.UseKubernetesHosting();
            }

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

            //todo how to switch down log level??
            //siloBuilder.AddLogging(builder=>builder.SetMinimumLevel(LogLevel.Debug);

            //todo streams cause kak so abandoning for now
            siloBuilder
                .AddSimpleMessageStreamProvider(StreamProvider.Transient, options => options.FireAndForgetDelivery = true)
                .AddAzureTableGrainStorage("PubSubStore", options => options.ConnectionString = clusterConfig.AzureStorageConnectionString);

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