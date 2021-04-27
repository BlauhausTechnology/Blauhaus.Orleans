using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.TestHelpers.Builders.Base;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseEntityGrainTest<TDbContext, TGrain, TEntity, TEntityBuilder> : BaseDbGrainTest<TGrain, TDbContext, Guid>
        where TGrain: BaseEntityGrain<TDbContext, TEntity> 
        where TEntity : class, IServerEntity
        where TEntityBuilder : IBuilder<TEntityBuilder, TEntity>
        where TDbContext : DbContext
    {
        protected TEntity ExistingEntity= null!;

        protected override void HandleSetup()
        {
            base.HandleSetup();
            GrainId = Guid.NewGuid();
        }

        protected override void SetupDbContext(TDbContext setupContext)
        {
            var entityBuilder = (TEntityBuilder)Activator.CreateInstance(typeof(TEntityBuilder), SetupTime);

            if (entityBuilder == null) throw new ArgumentNullException();

            SetupExistingEntity(entityBuilder);

            var entityToSave = entityBuilder.Object;
            ExistingEntity = setupContext.Seed(entityToSave);
            GrainId = ExistingEntity.Id;
        }
        
        protected override TGrain ConstructSut()
        {
            return Silo.CreateGrainAsync<TGrain>(GrainId).GetAwaiter().GetResult();
        }

        protected virtual void SetupExistingEntity(TEntityBuilder existingEntityBuilder)
        {
        }
    }
}