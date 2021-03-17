﻿using System;
using System.Threading.Tasks;
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

        protected IAsyncStream<T> GeTransientStream<T>(Guid streamId, string streamEventName)
        {
            var streamProvider = GetStreamProvider(StreamProvider.Transient);
            return streamProvider.GetStream<T>(streamId, streamEventName);
        }

        protected IAsyncStream<T> GetPersistentStream<T>(Guid streamId, string streamEventName)
        {
            var streamProvider = GetStreamProvider(StreamProvider.Persistent);
            return streamProvider.GetStream<T>(streamId, streamEventName);
        }

        protected async Task AddOrResumeTransientSubscriptionAsync<T>(Guid streamId, string streamEventName, Func<T, Task> handler)
        {
            var stream = this.GeTransientStream<T>(streamId, streamEventName);
            
            var existingHandles = await stream.GetAllSubscriptionHandles();
            if (existingHandles.Count == 0)
            {
                await stream.SubscribeAsync(async (t, token) =>
                {
                    await handler.Invoke(t);
                });
            }
            else
            {
                foreach (var streamSubscriptionHandle in existingHandles)
                {
                    await streamSubscriptionHandle.ResumeAsync(async (t, token) =>
                    { 
                        await handler.Invoke(t);
                    });
                }
            }
        }
    }
}