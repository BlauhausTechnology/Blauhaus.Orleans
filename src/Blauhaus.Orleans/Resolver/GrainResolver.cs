using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;

namespace Blauhaus.Orleans.Resolver
{
    public class GrainResolver : IGrainResolver
    {
        private Func<IGrainFactory>? _grainFactory;

        public TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey
        {
            if (_grainFactory == null)
            {
                throw new InvalidOperationException("GrainFactory has not been initialized");
            }

            return _grainFactory.Invoke().GetGrain<TGrain>(id);
        }

        public TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey
        {
            if (_grainFactory == null)
            {
                throw new InvalidOperationException("GrainFactory has not been initialized");
            }

            return _grainFactory.Invoke().GetGrain<TGrain>(id);
        }

        public TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey
        {
            if (_grainFactory == null)
            {
                throw new InvalidOperationException("GrainFactory has not been initialized");
            }

            return _grainFactory.Invoke().GetGrain<TGrain>(id);
        }

        public void Initialize(Func<IGrainFactory> initializer)
        {
            _grainFactory = initializer;
        }
    }
}