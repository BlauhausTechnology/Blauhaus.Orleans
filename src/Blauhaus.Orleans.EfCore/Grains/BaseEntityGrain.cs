using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Errors;
using Blauhaus.Auth.Abstractions.Extensions;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Domain.Abstractions.Commands;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Errors;
using Blauhaus.Orleans.Abstractions.Errors;
using Blauhaus.Orleans.Abstractions.Handlers;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.Resolver;
using Blauhaus.Responses;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseEntityGrain<TGrain, TDbContext, TEntity, TDto, TGrainResolver> : BaseEntityGrain<TGrain, TDbContext, TEntity, TGrainResolver>, IDtoOwner<TDto>
        where TDbContext : DbContext 
        where TEntity : BaseServerEntity
        where TDto : IClientEntity<Guid>
        where TGrainResolver : IGrainResolver
        where TGrain : BaseEntityGrain<TGrain, TDbContext, TEntity, TDto, TGrainResolver>
    {

        protected BaseEntityGrain(
            IAnalyticsLogger<TGrain> logger,
            Func<TDbContext> dbContextFactory,
            ITimeService timeService,
            TGrainResolver grainResolver)
                : base(logger, dbContextFactory, timeService, grainResolver)
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
                Logger.LogError(Error.Unexpected(), e);
                throw;
            }
        }

        protected abstract Task<TDto> GetDtoAsync(TEntity entity);

    }
    
    
    public abstract class BaseEntityGrain<TGrain, TDbContext, TEntity, TGrainResolver> : BaseDbGrain<TGrain, TDbContext, TGrainResolver>, IGrainWithGuidKey
        where TDbContext : DbContext 
        where TEntity : BaseServerEntity
        where TGrainResolver : IGrainResolver
        where TGrain : BaseDbGrain<TGrain, TDbContext, TGrainResolver>
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
            IAnalyticsLogger<TGrain> logger,
            Func<TDbContext> dbContextFactory, 
            ITimeService timeService,
            TGrainResolver grainResolver) 
                : base(dbContextFactory, logger, timeService, grainResolver)
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

                await using var context = GetDbContext();
                Entity = await LoadEntityAsync(context, Id);
                if (Entity != null)
                {
                    await HandleEntityLoadedAsync(context, Entity); 
                }
            }
            catch (Exception e)
            {
                Logger.LogError(GrainError.ActivationFailed, e);
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
        
        //public async Task<Response> HandleAsync(ActivateCommand command, IAuthenticatedUser user)
        //{
        //    return await TryExecuteDbCommandAsync(command, user, async (db, now) =>
        //    { 
        //        var currentEntityState = LoadedEntity.EntityState;
        //        if (currentEntityState is not (EntityState.Draft or EntityState.Archived))
        //        {
        //            return TraceError(DomainErrors.InvalidEntityState(currentEntityState));
        //        }

        //        db.Attach(LoadedEntity);
        //        LoadedEntity.Activate(now);
                
        //        return await HandleActivatedAsync(LoadedEntity);
        //    });
        //}

        //protected virtual Task<Response> HandleActivatedAsync(TEntity loadedEntity) => Response.SuccessTask();

        //public async Task<Response> HandleAsync(ArchiveCommand command, IAuthenticatedUser user)
        //{
        //    return await TryExecuteDbCommandAsync(command, user, async (db, now) =>
        //    { 
        //        var currentEntityState = LoadedEntity.EntityState;
        //        if (currentEntityState is not EntityState.Active)
        //        {
        //            return TraceError(DomainErrors.InvalidEntityState(currentEntityState));
        //        }

        //        db.Attach(LoadedEntity);
        //        LoadedEntity.Archive(now);

        //        return await HandleArchivedAsync(LoadedEntity);

        //    });
        //}

        //protected virtual Task<Response> HandleArchivedAsync(TEntity entity) => Response.SuccessTask();

        //public async Task<Response> HandleAsync(DeleteCommand command, IAuthenticatedUser user)
        //{
        //    return await TryExecuteDbCommandAsync(command, user, async (db, now) =>
        //    { 
        //        var currentEntityState = LoadedEntity.EntityState;
        //        if (currentEntityState is not (EntityState.Draft or EntityState.Archived))
        //        {
        //            return TraceError(DomainErrors.InvalidEntityState(currentEntityState));
        //        }

        //        db.Attach(LoadedEntity);
        //        LoadedEntity.Delete(now);
                
        //        return await HandleDeletedAsync(LoadedEntity); 
        //    });
        //}
        //protected virtual Task<Response> HandleDeletedAsync(TEntity entity) => Response.SuccessTask();
        

         
    }
}