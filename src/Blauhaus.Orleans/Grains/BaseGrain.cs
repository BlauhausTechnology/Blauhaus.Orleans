using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Orleans.Abstractions.Streams;
using Orleans;
using Orleans.Streams;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseGrain : Grain 
    {
        protected readonly IAnalyticsLogger Logger;

        protected BaseGrain(IAnalyticsLogger logger)
        {
            Logger = logger;
        }

        protected T GetGrain<T>(Guid id) where T : IGrainWithGuidKey => GrainFactory.GetGrain<T>(id);
        protected T GetGrain<T>(string id) where T : IGrainWithStringKey => GrainFactory.GetGrain<T>(id);
        
        protected IGrain GetGrain(Type grainInterfaceType, Guid id) => GrainFactory.GetGrain(grainInterfaceType, id);
        protected IGrain GetGrain(Type grainInterfaceType, string id) => GrainFactory.GetGrain(grainInterfaceType, id);

        protected IAsyncStream<T> GetTransientStream<T>(Guid streamId, string streamEventName)
        {
            var streamProvider = GetStreamProvider(StreamProvider.Transient);
            return streamProvider.GetStream<T>(streamId, streamEventName);
        }

        protected IAsyncStream<T> GetPersistentStream<T>(Guid streamId, string streamEventName)
        {
            var streamProvider = GetStreamProvider(StreamProvider.Persistent);
            return streamProvider.GetStream<T>(streamId, streamEventName);
        }

        protected async Task PublishAsync<T>(Guid streamId, string streamEventName, T t)
        {
            var stream = GetTransientStream<T>(streamId, streamEventName);
            await stream.OnNextAsync(t);
        }
        
        
      protected async Task PublishToStreamAsync<T>(string streamName, Guid streamId, string? streamEventName, T t)
        {
            var streamProvider = GetStreamProvider(streamName);
            var stream = streamProvider.GetStream<T>(streamId, streamEventName);
            await stream.OnNextAsync(t);
        }

        protected async Task SubscribeToStreamAsync<T>(string streamName, Guid streamId, string? streamEventName, Func<T, Task> handler)
        {
            var streamProvider = GetStreamProvider(streamName);
            var stream = streamProvider.GetStream<T>(streamId, streamEventName);

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

        protected async Task UnsubscribeFromStreamAsync<T>(string streamName, Guid streamId, string? streamEventName = null)
        {
            var streamProvider = GetStreamProvider(streamName);
            var stream = streamProvider.GetStream<T>(streamId, streamEventName);

            var existingHandles = await stream.GetAllSubscriptionHandles();

            foreach (var streamSubscriptionHandle in existingHandles)
            {
                await streamSubscriptionHandle.UnsubscribeAsync();
            }
        }
    }
}