using System;
using Blauhaus.Orleans.Abstractions.Streams;
using Orleans;
using Orleans.Streams;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseGrain : Grain
    {
        protected TGrain GetGrain<TGrain>(Guid id) where TGrain : IGrainWithGuidKey => GrainFactory.GetGrain<TGrain>(id);
        protected TGrain GetGrain<TGrain>(string id) where TGrain : IGrainWithStringKey => GrainFactory.GetGrain<TGrain>(id);
        protected IGrain GetGrain(Type grainInterfaceType, Guid id) => GrainFactory.GetGrain(grainInterfaceType, id);
        protected IGrain GetGrain(Type grainInterfaceType, string id) => GrainFactory.GetGrain(grainInterfaceType, id);

        protected IAsyncStream<T> GeTransientStream<T>(Guid id, string name)
        {
            var streamProvider = GetStreamProvider(StreamProvider.Transient);
            return streamProvider.GetStream<T>(id, name);
        }

        protected IAsyncStream<T> GetPersistentStream<T>(Guid id, string name)
        {
            var streamProvider = GetStreamProvider(StreamProvider.Persistent);
            return streamProvider.GetStream<T>(id, name);
        }
        
        
    }
}