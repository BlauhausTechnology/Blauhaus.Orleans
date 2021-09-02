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
        where TEntityBuilder : IBuilder<TEntityBuilder, TEntity>
        where TDbContext : DbContext
        where TGrainResolver : IGrainResolver
    {
        protected TEntity ExistingEntity = null!;

        protected override void SetupDbContext(TDbContext setupContext)
        {
            var entityBuilderObject = Activator.CreateInstance(typeof(TEntityBuilder), SetupTime);
            if (entityBuilderObject == null) throw new ArgumentNullException();

            var entityBuilder = (TEntityBuilder)entityBuilderObject;
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