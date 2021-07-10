using Blauhaus.Orleans.Abstractions.Grains;
using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseResolverGrain<TGrainResolver> : Grain where TGrainResolver : IGrainResolver
    {
        protected readonly TGrainResolver GrainResolver;
        
        protected BaseResolverGrain(TGrainResolver grainResolver)
        {
            GrainResolver = grainResolver;
            GrainResolver.Initialize(()=> GrainFactory);
        }

        protected TGrain ResolveSingleton<TGrain>() where TGrain : IGrainSingleton
        {
            return GrainResolver.ResolveSingleton<TGrain>();
        }
        protected TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey
        {
            return GrainResolver.Resolve<TGrain>(id);
        }
        protected TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey
        {
            return GrainResolver.Resolve<TGrain>(id);
        }
        protected TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey
        {
            return GrainResolver.Resolve<TGrain>(id);
        }

    }
}