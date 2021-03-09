using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.TestHelpers.Builders._Base;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseEntityGrainTest<TDbContext, TGrain, TEntity, TEntityBuilder> : BaseDbGrainTest<TGrain, TDbContext>
        where TGrain: BaseEntityGrain<TDbContext, TEntity> 
        where TEntity : class, IServerEntity
        where TEntityBuilder : IBuilder<TEntityBuilder, TEntity>
        where TDbContext : DbContext
    {
        protected TEntity ExistingEntity;

        protected override void SetupDbContext(TDbContext setupContext)
        {
            var entityBuilder = (TEntityBuilder)Activator.CreateInstance(typeof(TEntityBuilder), SetupTime);

            if (entityBuilder == null) throw new ArgumentNullException();

            SetupExistingEntity(entityBuilder);

            var entityToSave = entityBuilder.Object;
            ExistingEntity = setupContext.Seed(entityToSave);
            GrainId = ExistingEntity.Id;
        }

        protected virtual void SetupExistingEntity(TEntityBuilder existingEntityBuilder)
        {
        }
    }
}