using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Orleans.Abstractions.Handlers;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Abstractions.Server.Handlers;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseConnectedUserGrain<TDbContext, TEntity, TDto> : BaseConnectedUserGrain<TDbContext, TEntity>, IDtoLoader<TDto>
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity, IHasUserId
        where TDto : IClientEntity
    {
        
        protected BaseConnectedUserGrain(Func<TDbContext> dbContextFactory, IAnalyticsService analyticsService, ITimeService timeService) : base(dbContextFactory, analyticsService, timeService)
        {
        }
        
        public Task<TDto> GetDtoAsync()
        {
            if (Entity == null)
            {
                throw new InvalidOperationException($"Entity {Id} does not exist");
            }
            
            return Task.FromResult(GetDto(Entity));
        }
        
        protected abstract TDto GetDto(TEntity entity);

    }
    
    
    
    public abstract class BaseConnectedUserGrain<TDbContext, TEntity> : BaseEntityGrain<TDbContext, TEntity>, IConnectedUserHandler
        where TEntity : class, IServerEntity, IHasUserId 
        where TDbContext : DbContext
    {
        
        protected readonly Dictionary<string, IConnectedUser> UserConnections = new();
        
        protected BaseConnectedUserGrain(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
                : base(dbContextFactory, analyticsService, timeService)
        {
        }

        protected sealed override async Task LoadDependentEntitiesAsync(TDbContext dbContext, TEntity entity)
        {
            await base.LoadDependentEntitiesAsync(dbContext, entity);

            await AddOrResumeTransientSubscriptionAsync<IConnectedUser>(entity.UserId, ConnectedUserEvents.UserConnected, user =>
                {
                    if (UserConnections.TryGetValue(user.UniqueId, out _))
                    {
                        UserConnections[user.UniqueId] = user;
                        return HandleConnectedUserAsync(user);
                    }
                    return Task.CompletedTask;
                });
            
            await AddOrResumeTransientSubscriptionAsync<IConnectedUser>(entity.UserId, ConnectedUserEvents.UserDisconnected, user => 
            {
                if (UserConnections.TryGetValue(user.UniqueId, out _))
                {
                    UserConnections.Remove(user.UniqueId);
                    return HandleDisconnectedUserAsync(user);
                }
                return Task.CompletedTask;
            });

            await LoadUserDependentEntitiesAsync(dbContext, entity);
        }

        protected virtual Task LoadUserDependentEntitiesAsync(TDbContext dbContext, TEntity entity)
        {
            return Task.CompletedTask;
        }
        
        [OneWay]
        public Task ConnectUserAsync(IConnectedUser user)
        {
            if (UserConnections.TryGetValue(user.UniqueId, out _))
            {
                UserConnections[user.UniqueId] = user;
                return HandleConnectedUserAsync(user);
            }
            return Task.CompletedTask;
        }

        protected virtual Task HandleConnectedUserAsync(IConnectedUser user)
        {
            return Task.CompletedTask;
        }

        [OneWay]
        public Task DisconnectUserAsync(IConnectedUser user)
        {
            if (UserConnections.TryGetValue(user.UniqueId, out _))
            {
                UserConnections.Remove(user.UniqueId);
                return HandleDisconnectedUserAsync(user);
            }
            return Task.CompletedTask;
        }
        
        protected virtual Task HandleDisconnectedUserAsync(IConnectedUser user)
        {
            return Task.CompletedTask;
        }
    }
}