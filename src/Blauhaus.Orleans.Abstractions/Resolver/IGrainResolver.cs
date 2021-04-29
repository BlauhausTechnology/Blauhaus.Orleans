using System;
using Orleans;

namespace Blauhaus.Orleans.Abstractions.Resolver
{
    public interface IGrainResolver
    {
        void Initialize(Func<IGrainFactory> initializer);
        
        TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey;
        TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey;
        TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey;
    }
}