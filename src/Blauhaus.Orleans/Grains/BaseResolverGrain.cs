using Blauhaus.Orleans.Abstractions.Grains;
using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseResolverGrain<TGrainResolver> : Grain where TGrainResolver : IGrainResolver
    {
        private readonly TGrainResolver _grainResolver;
        
        protected BaseResolverGrain(TGrainResolver grainResolver)
        {
            _grainResolver = grainResolver;
            _grainResolver.Initialize(()=> GrainFactory);
        }

        protected TGrain ResolveSingleton<TGrain>() where TGrain : IGrainSingleton
        {
            return _grainResolver.ResolveSingleton<TGrain>();
        }
        protected TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey
        {
            return _grainResolver.Resolve<TGrain>(id);
        }
        protected TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey
        {
            return _grainResolver.Resolve<TGrain>(id);
        }
        protected TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey
        {
            return _grainResolver.Resolve<TGrain>(id);
        }

    }
}