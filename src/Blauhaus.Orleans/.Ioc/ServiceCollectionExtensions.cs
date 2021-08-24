using Blauhaus.Common.ValueObjects.BuildConfigs;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.ClusterClient;
using Blauhaus.Orleans.Config;
using Blauhaus.Orleans.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Orleans;

namespace Blauhaus.Orleans.Ioc
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddGrainResolver<TResolver, TResolverImplementation>(this IServiceCollection services)
            where TResolver : class, IGrainResolver
            where TResolverImplementation : class, TResolver
        {
            services.AddScoped<TResolver, TResolverImplementation>();
            services.AddScoped<IGrainResolver>(sp => sp.GetRequiredService<TResolverImplementation>());
            return services;
        }

        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, IConfiguration configuration, string clusterName)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var buildConfig = configuration.ExtractBuildConfig();

            services.AddSingleton<IOrleansConfig>(x => new ClientConfig(connectionString, clusterName, buildConfig));

            services.AddSingleton<HostedClusterClient>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<HostedClusterClient>());
            services.AddSingleton<IClusterClient>(sp => sp.GetRequiredService<HostedClusterClient>().Client);
            services.AddSingleton<IGrainFactory>(sp => sp.GetRequiredService<HostedClusterClient>().Client);

            return services;
        }


        public static IServiceCollection AddOrleansClusterClient<TConfig>(this IServiceCollection services) where TConfig : class, IOrleansConfig
        {

            services.TryAddSingleton<IOrleansConfig, TConfig>();

            services.AddSingleton<HostedClusterClient>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<HostedClusterClient>());
            services.AddSingleton<IClusterClient>(sp => sp.GetRequiredService<HostedClusterClient>().Client);
            services.AddSingleton<IGrainFactory>(sp => sp.GetRequiredService<HostedClusterClient>().Client);

            return services;
        }
    }
}