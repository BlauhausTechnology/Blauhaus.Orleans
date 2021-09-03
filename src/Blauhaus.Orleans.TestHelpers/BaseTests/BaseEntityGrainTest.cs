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
        private TEntity? _existingEntity;
        protected TEntity ExistingEntity
        {
            get
            {
                if (_existingEntity == null)
                {
                    throw new InvalidOperationException("ExistingEntity is only set when the test executes");
                }

                return _existingEntity;
            }
        }

        protected TEntityBuilder ExistingEntityBuilder = null!;

        public override void Setup()
        {
            base.Setup();

            GrainId = Guid.NewGuid();
            ExistingEntityBuilder = (TEntityBuilder) Activator.CreateInstance(typeof(TEntityBuilder), SetupTime)!;
            ExistingEntityBuilder.With(x => x.Id, GrainId);
            
            AddEntityBuilder(ExistingEntityBuilder);
        }

        protected override TGrain ConstructGrain()
        {
            _existingEntity = ExistingEntityBuilder.Object;
            return Silo.CreateGrainAsync<TGrain>(GrainId).GetAwaiter().GetResult();
        }
          
    }
}