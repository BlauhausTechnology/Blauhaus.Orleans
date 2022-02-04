using System;
using Blauhaus.Orleans.Abstractions.Grains;
using Blauhaus.Orleans.Abstractions.Identity;
using Orleans;

namespace Blauhaus.Orleans.Abstractions.Resolver
{
    public interface IGrainResolver
    {
        void Initialize(Func<IGrainFactory> initializer);
        
        TGrain ResolveSingleton<TGrain>() where TGrain : IGrainSingleton;
        TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey;
        TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey;
        TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey;

        TGrain Resolve<TGrain>(GrainId grainId) where TGrain : class, IGrainWithStringKey;
        TGrain Resolve<TGrain, TId>(TId seralizableId) where TGrain : class, IGrainWithStringKey;
    }
}