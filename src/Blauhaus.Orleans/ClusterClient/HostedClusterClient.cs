﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Common.ValueObjects.BuildConfigs;
using Blauhaus.Orleans.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Blauhaus.Orleans.ClusterClient
{
    public class HostedClusterClient : IHostedService
    {
        public HostedClusterClient(
            IBuildConfig buildConfig,
            IOrleansConfig clusterConfig)
        {
            
            var clientBuilder = new ClientBuilder()
                .UseAzureStorageClustering(options =>
                {
                    options.ConnectionString = clusterConfig.AzureStorageConnectionString;
                    options.TableName = clusterConfig.StorageTableName;
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ServiceId = clusterConfig.ServiceId;
                    options.ClusterId = clusterConfig.ClusterId;
                });

            if (buildConfig.Equals(BuildConfig.Debug))
            {
                clientBuilder
                    .Configure<ClusterMembershipOptions>(x => x.ValidateInitialConnectivity = false);
            }

            Client = clientBuilder.Build();
        }

        
        public IClusterClient Client { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Client.Connect(async x =>
            {
                Console.WriteLine("Connection failed due to " + x.Message + ". Reconnecting in 5s...");
                await Task.Delay(5000);
                return true;
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.Close();
            Client.Dispose();
        }
    }
}