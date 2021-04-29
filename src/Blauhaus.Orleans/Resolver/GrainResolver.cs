using System;
using Blauhaus.Orleans.Abstractions.Grains;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;

namespace Blauhaus.Orleans.Resolver
{
    public class GrainResolver : IGrainResolver
    {
        private Func<IGrainFactory>? _grainFactory;


        public TGrain ResolveSingleton<TGrain>() where TGrain : IGrainSingleton
        {
            return GetGrainFactory()
                .GetGrain<TGrain>(Guid.Empty);
        }

        public TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey
        {
            return GetGrainFactory()
                .GetGrain<TGrain>(id);
        }

        public TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey
        {
            return GetGrainFactory()
                .GetGrain<TGrain>(id);
        }

        public TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey
        {
            return GetGrainFactory()
                .GetGrain<TGrain>(id);
        }

        public void Initialize(Func<IGrainFactory> initializer)
        {
            _grainFactory = initializer;
        }

        private IGrainFactory GetGrainFactory()
        {
            if (_grainFactory == null)
            {
                throw new InvalidOperationException("GrainFactory has not been initialized");
            }

            return _grainFactory.Invoke();
        }

    }
}