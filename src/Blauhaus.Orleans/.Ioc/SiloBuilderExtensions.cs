using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using AutoMapper.Configuration;
using Blauhaus.Common.ValueObjects.BuildConfigs;
using Blauhaus.Orleans.Abstractions.Streams;
using Blauhaus.Orleans.Config;
using Blauhaus.Orleans.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

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

        public static ISiloBuilder ConfigureTransientStreams(this ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            siloBuilder
                .AddSimpleMessageStreamProvider(StreamProvider.Transient, options => options.FireAndForgetDelivery = true)
                .AddAzureTableGrainStorage("PubSubStore", options =>
                {
                    options.ConfigureTableServiceClient(configuration.GetConnectionString("AzureStorage"));
                    options.UseJson = true;
                });

            return siloBuilder;
        }

        public static ISiloBuilder ConfigurePersistentStreams(this ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            var pullingPeriodConfig = configuration.ExtractClusterValue("PersistentStreamPullingPeriodMs");
            if (!int.TryParse(pullingPeriodConfig, out var pullingPeriod))
            {
                pullingPeriod = 5000;
            }
            siloBuilder
                .AddAzureQueueStreams(StreamProvider.Persistent, configurator =>
                {
                    configurator.ConfigureAzureQueue(
                        ob => ob.Configure(options =>
                        {
                            options.ConfigureQueueServiceClient(configuration.GetConnectionString("AzureStorage"));
                            options.QueueNames = new List<string> { "azurequeueprovider-0" };
                        }));
                    configurator.ConfigureCacheSize(1024);
                    configurator.ConfigurePullingAgent(ob => ob.Configure(options =>
                    {
                        options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(pullingPeriod);
                    }));
                });

            return siloBuilder;
        }
        
        public static ISiloBuilder ConfigureClustering(this ISiloBuilder siloBuilder, IConfiguration configuration, string clusterName)
        {
            
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var buildConfig = configuration.ExtractBuildConfig();

            siloBuilder.UseAzureStorageClustering(options =>
            {
                options.ConfigureTableServiceClient(connectionString);
                options.TableName = clusterName + "ClusterInfo";
            });
             
            if (buildConfig.Equals(BuildConfig.Debug))
            {
                siloBuilder.ConfigureEndpoints(Dns.GetHostName(), 11111, 30000);
                siloBuilder.Configure<ClusterMembershipOptions>(x => x.ValidateInitialConnectivity = false);
                siloBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ServiceId = $"{clusterName}Service";
                    options.ClusterId = $"{clusterName}Cluster";
                });
            }
            else
            {
                siloBuilder
                    .UseKubernetesHosting();
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
                    options.ConfigureTableServiceClient(clusterConfig.AzureStorageConnectionString);
                    options.TableName = clusterConfig.StorageTableName;
                })
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddFromApplicationBaseDirectory(); 
                    parts.AddApplicationPart(grainAssembly).WithReferences();
                });

            siloBuilder
                .AddSimpleMessageStreamProvider(StreamProvider.Transient, options => options.FireAndForgetDelivery = true)
                .AddAzureTableGrainStorage("PubSubStore", options => 
                    options.ConfigureTableServiceClient(clusterConfig.AzureStorageConnectionString));

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