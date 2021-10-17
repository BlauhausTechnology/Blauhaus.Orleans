using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Orleans.Abstractions.Handlers;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.Resolver;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseEntityGrain<TDbContext, TEntity, TDto, TGrainResolver> : BaseEntityGrain<TDbContext, TEntity, TDto, TGrainResolver, Guid>
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity
        where TDto : IClientEntity<Guid>
        where TGrainResolver : IGrainResolver
    {
        protected BaseEntityGrain(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService, 
            TGrainResolver grainResolver) 
                : base(dbContextFactory, analyticsService, timeService, grainResolver)
        {
        }
    }

    
    //this is temporary to accomodate moonbase with Guid ids and string dto ids
    public abstract class BaseEntityGrain<TDbContext, TEntity, TDto, TGrainResolver, TDtoId> : BaseEntityGrain<TDbContext, TEntity, TGrainResolver>, IDtoOwner<TDto>
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity
        where TDto : IClientEntity<TDtoId>
        where TGrainResolver : IGrainResolver
    {
        
        protected BaseEntityGrain(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService,
            TGrainResolver grainResolver) 
                : base(dbContextFactory, analyticsService, timeService, grainResolver)
        {
        } 
        
        public Task<TDto> GetDtoAsync()
        {
            try
            {
                if (Entity == null)
                {
                    throw new InvalidOperationException($"Cannot GetDto because {typeof(TEntity).Name} with id {Id} does not exist");
                }
            
                return GetDtoAsync(Entity);
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
                throw;
            }
        }

        protected abstract Task<TDto> GetDtoAsync(TEntity entity);

    }
    
    
    public abstract class BaseEntityGrain<TDbContext, TEntity, TGrainResolver> : BaseDbGrain<TDbContext, TGrainResolver>, IGrainWithGuidKey
        where TDbContext : DbContext 
        where TEntity : class, IServerEntity
        where TGrainResolver : IGrainResolver
    {
        protected TEntity? Entity;

        protected TEntity LoadedEntity
        {
            get
            {
                if (Entity == null)
                {
                    throw new InvalidOperationException("Entity has not been loaded or does not exist");
                }

                return Entity;
            }
        }

        protected Guid Id;
        
        protected BaseEntityGrain(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService,
            TGrainResolver grainResolver) 
                : base(dbContextFactory, analyticsService, timeService, grainResolver)
        {
        }
         
        public override async Task OnActivateAsync()
        {
            try
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
                        await HandleEntityLoadedAsync(context, Entity); 
                    }
                }
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
                throw;
            }
        }

        protected virtual async Task<TEntity?> LoadEntityAsync(TDbContext dbContext, Guid id)
        { 
            var query = dbContext.Set<TEntity>().AsNoTracking();

            query = Include(query);
            
            return await query
                .FirstOrDefaultAsync(GetFilter(id));
        }

        protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query)
        {
            return query;
        }

        protected Expression<Func<TEntity, bool>> GetFilter(Guid id)
        {
            return entity => entity.Id == id;
        }

        protected virtual Task HandleEntityLoadedAsync(TDbContext dbContext, TEntity entity)
        {
            return Task.CompletedTask;
        }
         
    }
}