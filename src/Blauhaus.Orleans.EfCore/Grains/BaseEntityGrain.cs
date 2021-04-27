using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Orleans.Abstractions.Handlers;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseEntityGrain<TDbContext, TEntity, TDto> : BaseEntityGrain<TDbContext, TEntity>, IDtoLoader<TDto>
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity
        where TDto : IClientEntity
    {
        
        protected BaseEntityGrain(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
                : base(dbContextFactory, analyticsService, timeService)
        {
        }
        
        public Task<TDto> GetDtoAsync()
        {
            if (Entity == null)
            {
                throw new InvalidOperationException($"Cannot GetDto because {typeof(TEntity).Name} with id {Id} does not exist");
            }
            
            return GetDtoAsync(Entity);
        }

        protected abstract Task<TDto> GetDtoAsync(TEntity entity);

    }
    
    
    public abstract class BaseEntityGrain<TDbContext, TEntity> : BaseDbGrain<TDbContext> 
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity
    {
        protected TEntity? Entity;
        protected Guid Id;
        
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

            Id = this.GetPrimaryKey();

            if (Id == Guid.Empty)
            {
                throw new ArgumentException($"Grain requires a GUID id. \"{this.GetPrimaryKey()}\" is not valid");
            }

            await using (var context = GetDbContext())
            {
                Entity = await LoadEntityAsync(context, Id);
                if (Entity != null)
                {
                    await HandleEntitiesLoadedAsync(context, Entity);
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

        protected virtual Task HandleEntitiesLoadedAsync(TDbContext dbContext, TEntity entity)
        {
            return Task.CompletedTask;
        }

        [Obsolete("Use HandleEntitiesLoadedAsync instead")]
        protected virtual Task LoadDependentEntitiesAsync(TDbContext dbContext, TEntity entity)
        {
            return Task.CompletedTask;
        }
    }
}