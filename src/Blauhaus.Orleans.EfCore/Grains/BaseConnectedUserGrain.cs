using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Abstractions.Server.Handlers;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseConnectedUserGrain<TDbContext, TEntity> : BaseEntityGrain<TDbContext, TEntity>, IConnectedUserHandler
        where TEntity : class, IServerEntity 
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

        [OneWay]
        public Task ConnectUserAsync(IConnectedUser user)
        { 
            UserConnections[$"{user.UserId}|{user.CurrentDeviceIdentifier}"] = user;
            return HandleConnectedUserAsync(user);
        }

        protected virtual Task HandleConnectedUserAsync(IConnectedUser user)
        {
            return Task.CompletedTask;
        }

        [OneWay]
        public Task DisconnectUserAsync(IConnectedUser user)
        {
            if (UserConnections.TryGetValue($"{user.UserId}|{user.CurrentDeviceIdentifier}", out var existingConnection))
            {
                UserConnections.Remove($"{user.UserId}|{user.CurrentDeviceIdentifier}");
            }
            return HandleDisconnectedUserAsync(user);
        }
        
        protected virtual Task HandleDisconnectedUserAsync(IConnectedUser user)
        {
            return Task.CompletedTask;
        }
    }
}