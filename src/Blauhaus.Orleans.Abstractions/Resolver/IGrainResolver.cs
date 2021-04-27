using System;
using Orleans;

namespace Blauhaus.Orleans.Abstractions.Resolver
{
    public interface IGrainResolver
    {
        void Initialize(Func<IGrainFactory> initializer);
        
        TGrain GetGrain<TGrain>(Guid id) where TGrain : IGrainWithGuidKey;
        TGrain GetGrain<TGrain>(string id) where TGrain : IGrainWithStringKey;
        TGrain GetGrain<TGrain>(long id) where TGrain : IGrainWithIntegerKey;
    }
}