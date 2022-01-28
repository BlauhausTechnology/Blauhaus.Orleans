using System;
using Blauhaus.Orleans.Abstractions.Grains;
using Blauhaus.Orleans.Abstractions.Identity;
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

        public TGrain Resolve<TGrain>(GrainId grainId) where TGrain : class, IGrainWithGuidKey
        {
            var grainInterfaceType = Type.GetType(grainId.InterfaceTypeName);
            if (grainInterfaceType == null)
            {
                throw new InvalidOperationException($"Unable to get Type from name {grainId.InterfaceTypeName} for grain id {grainId.Id}");
            }

            var grain = GetGrainFactory().GetGrain(grainInterfaceType, grainId.Id);
            if (grain == null)
            {
                throw new InvalidOperationException($"Unable to get Grain for {grainInterfaceType.Name} with id {grainId.Id}");
            }

            if (grain is not TGrain grainAsRequiredType)
            {
                throw new InvalidOperationException($"Unable to cast Grain of type {grain.GetType().Name} to {typeof(TGrain)}");
            }

            return grainAsRequiredType;
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