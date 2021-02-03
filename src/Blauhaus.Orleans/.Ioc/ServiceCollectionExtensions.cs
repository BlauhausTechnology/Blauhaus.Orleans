using Blauhaus.Orleans.ClusterClient;
using Blauhaus.Orleans.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Orleans;

namespace Blauhaus.Orleans.Ioc
{
    public static class ServiceCollectionExtensions
    {
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