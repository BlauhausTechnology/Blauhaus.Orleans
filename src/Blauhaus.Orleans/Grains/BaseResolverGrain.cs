using Blauhaus.Orleans.Abstractions.Grains;
using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;
using Blauhaus.Analytics.Abstractions;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseResolverGrain<TGrainResolver> : BaseGrain
        where TGrainResolver : IGrainResolver
    {
        protected readonly TGrainResolver GrainResolver;
        
        protected BaseResolverGrain(
            IAnalyticsLogger logger,
            TGrainResolver grainResolver) : base(logger)
        {
            GrainResolver = grainResolver;
            GrainResolver.Initialize(()=> GrainFactory);
        }

        protected T ResolveSingleton<T>() where T : IGrainSingleton
        {
            return GrainResolver.ResolveSingleton<T>();
        }
        protected T Resolve<T>(Guid id) where T : IGrainWithGuidKey
        {
            return GrainResolver.Resolve<T>(id);
        }
        protected T Resolve<T>(string id) where T : IGrainWithStringKey
        {
            return GrainResolver.Resolve<T>(id);
        }
        protected T Resolve<T>(long id) where T : IGrainWithIntegerKey
        {
            return GrainResolver.Resolve<T>(id);
        } 

        

    }
}