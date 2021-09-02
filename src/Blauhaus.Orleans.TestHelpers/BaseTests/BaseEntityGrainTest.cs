using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.TestHelpers.Builders.Base;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseEntityGrainTest<TDbContext, TGrain, TEntity, TEntityBuilder, TGrainResolver> : BaseDbGrainTest<TGrain, TDbContext, Guid, TGrainResolver>
        where TGrain: BaseEntityGrain<TDbContext, TEntity, TGrainResolver> 
        where TEntity : BaseServerEntity
        where TEntityBuilder : BaseReadonlyFixtureBuilder<TEntityBuilder, TEntity>
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
            
            AddEntityBuilder(ExistingEntityBuilder);
             
        }

        protected override TGrain ConstructGrain()
        {
            return Silo.CreateGrainAsync<TGrain>(GrainId).GetAwaiter().GetResult();
        }
         
        [Obsolete("Use ExistingEntityBuilder instead")]
        protected virtual void SetupExistingEntity(TEntityBuilder existingEntityBuilder)
        {
        }
    }
}