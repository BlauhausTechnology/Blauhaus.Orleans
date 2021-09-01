using System;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Server.EFCore.Repositories;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.Abstractions.Streams;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Streams;

namespace Blauhaus.Orleans.EfCore.Clients
{
    public abstract class BaseClusterClientDtoLoader<TDbContext, TDto, TEntity, TDtoId, TEntityId, TGrainResolver> : BaseServerDtoLoader<TDbContext, TDto, TEntity, TDtoId>
        where TDbContext : DbContext
        where TDto : class, IClientEntity<TDtoId>
        where TEntity : class, IServerEntity 
        where TEntityId : IEquatable<TEntityId>
        where TDtoId : IEquatable<TDtoId>
        where TGrainResolver : IGrainResolver
    {

        private readonly IClusterClient _clusterClient;

        protected readonly TGrainResolver GrainResolver;
        
        protected TGrain Resolve<TGrain>(Guid id) where TGrain : IGrainWithGuidKey 
            => GrainResolver.Resolve<TGrain>(id);
        protected TGrain Resolve<TGrain>(string id) where TGrain : IGrainWithStringKey 
            => GrainResolver.Resolve<TGrain>(id);

        protected IAsyncStream<T> GetStream<T>(Guid streamId, string streamName)
            => _clusterClient.GetStreamProvider(StreamProvider.Transient).GetStream<T>(streamId, streamName);
        protected IAsyncStream<T> GetDtoStream<T>(Guid streamId) where T : IClientEntity<TDtoId>
            => _clusterClient.GetStreamProvider(StreamProvider.Transient).GetStream<T>(streamId, $"{typeof(T).Name}Updated");

        protected BaseClusterClientDtoLoader(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService, 
            TGrainResolver grainResolver,
            IClusterClient clusterClient) 
                : base(dbContextFactory, analyticsService, timeService)
        { 
            GrainResolver = grainResolver;
            _clusterClient = clusterClient;

            GrainResolver.Initialize(()=> clusterClient);
        }

        public async Task InitializeAsync()
        {
            try
            {
                await SubscribeToDtoUpdatedAsync();
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
                AnalyticsService.TraceWarning(this, $"DtoUpdated subscription failed for {typeof(TDto).Name}. Retrying in 5 seconds...");
                await Task.Delay(5000);
                await SubscribeToDtoUpdatedAsync();
            }
        }

        private async Task SubscribeToDtoUpdatedAsync()
        {
            var streamName = $"{typeof(TDto).Name}Updated";
            var streamProvider = _clusterClient.GetStreamProvider(StreamProvider.Transient);
            var dtoUpdatedStream = streamProvider.GetStream<TDto>(Guid.Empty, streamName);
            var dtoUpdatedSubscriptions = await dtoUpdatedStream.GetAllSubscriptionHandles();
            if (dtoUpdatedSubscriptions.Any())
            {
                foreach (var streamSubscriptionHandle in dtoUpdatedSubscriptions)
                {
                    AnalyticsService.Debug($"Resuming {streamName} subscription");
                    await streamSubscriptionHandle.ResumeAsync(HandleDtoUpdatedAsync);
                }
            }
            else
            {
                AnalyticsService.Debug($"Initializing {streamName} subscription");
                await dtoUpdatedStream.SubscribeAsync(HandleDtoUpdatedAsync);
            }
        }

        private async Task HandleDtoUpdatedAsync(TDto dto, StreamSequenceToken token)
        {
            await UpdateSubscribersAsync(dto);
            await HandleDtoUpdatedAsync(dto);
        }

        protected virtual Task HandleDtoUpdatedAsync(TDto dto)
        {
            return Task.CompletedTask;
        }
    }
}