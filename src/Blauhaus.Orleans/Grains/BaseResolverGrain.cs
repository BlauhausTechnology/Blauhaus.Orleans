using Blauhaus.Orleans.Abstractions.Grains;
using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;
using System.Threading.Tasks;
using Blauhaus.Orleans.Abstractions.Streams;
using Orleans.Streams;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseResolverGrain<TGrainResolver> : Grain
        where TGrainResolver : IGrainResolver
    {
        protected readonly TGrainResolver GrainResolver;
        
        protected BaseResolverGrain(TGrainResolver grainResolver)
        {
            GrainResolver = grainResolver;
            GrainResolver.Initialize(()=> GrainFactory);
        }

        protected TGrain ResolveSingleton<TGrain>() where TGrain : IGrainSingleton
        {
            return GrainResolver.ResolveSingleton<TGrain>();
        }
        protected TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey
        {
            return GrainResolver.Resolve<TGrain>(id);
        }
        protected TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey
        {
            return GrainResolver.Resolve<TGrain>(id);
        }
        protected TGrain Resolve<TGrain>(long id) where TGrain : IGrainWithIntegerKey
        {
            return GrainResolver.Resolve<TGrain>(id);
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