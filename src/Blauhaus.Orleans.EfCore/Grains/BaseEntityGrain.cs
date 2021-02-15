﻿using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseEntityGrain<TDbContext, TEntity> : BaseDbGrain<TDbContext> 
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity
    {
        protected TEntity? Entity;
        
        protected BaseEntityGrain(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
                : base(dbContextFactory, analyticsService, timeService)
        {
        }
        
        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            await using (var context = GetDbContext())
            {
                Entity = await LoadEntityAsync(context, Id);
                if (Entity != null)
                {
                    await LoadDependentEntitiesAsync(context, Entity);
                }
            }
        }

        protected virtual async Task<TEntity?> LoadEntityAsync(TDbContext dbContext, Guid id)
        {
            return await dbContext.Set<TEntity>().AsNoTracking()
                .FirstOrDefaultAsync(x => 
                    x.Id == id && 
                    x.EntityState != EntityState.Deleted);
        }

        protected virtual Task LoadDependentEntitiesAsync(TDbContext dbContext, TEntity entity)
        {
            return Task.CompletedTask;
        }
    }
}