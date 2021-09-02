using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.TestHelpers.Builders.Base;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseEntityGrainTest<TDbContext, TGrain, TEntity, TEntityBuilder, TGrainResolver> : BaseDbGrainTest<TGrain, TDbContext, Guid, TGrainResolver>
        where TGrain: BaseEntityGrain<TDbContext, TEntity, TGrainResolver> 
        where TEntity : class, IServerEntity
        where TEntityBuilder : class, IBuilder<TEntityBuilder, TEntity>
        where TDbContext : DbContext
        where TGrainResolver : IGrainResolver
    {
        protected TEntity ExistingEntity => ExistingEntityBuilder.Object;
        protected TEntityBuilder ExistingEntityBuilder = null!;

        protected override void SetupDbContext(TDbContext setupContext)
        {
            GrainId = Guid.NewGuid();

            ExistingEntityBuilder = (TEntityBuilder) Activator.CreateInstance(typeof(TEntityBuilder), SetupTime)!;

            ExistingEntityBuilder.With(x => x.Id, GrainId);

            SetupExistingEntity(ExistingEntityBuilder);
             
        }
        
        protected override TGrain ConstructSut()
        {
            Seed(ExistingEntityBuilder.Object);
            return Silo.CreateGrainAsync<TGrain>(GrainId).GetAwaiter().GetResult();
        }

        protected virtual void SetupExistingEntity(TEntityBuilder existingEntityBuilder)
        {
        }
    }
}