using Blauhaus.Auth.Common._Ioc;
using Blauhaus.Orleans.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blauhaus.Orleans._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOrleansRequestContext(this IServiceCollection services)
        {
            services.TryAddSingleton<IRequestContext, RequestContextProxy>();
            services.AddAzureUserFactory();
        }
    }
}