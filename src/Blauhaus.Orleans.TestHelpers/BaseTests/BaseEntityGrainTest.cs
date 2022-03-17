using System;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EntityBuilders;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseEntityGrainTest<TDbContext, TGrain, TEntity, TEntityBuilder, TGrainResolver> : BaseDbGrainTest<TGrain, TDbContext, Guid, TGrainResolver>
        where TGrain: BaseEntityGrain<TDbContext, TEntity, TGrainResolver> 
        where TEntity : BaseServerEntity
        where TEntityBuilder : BaseServerEntityBuilder<TEntityBuilder, TEntity>
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

            ExistingEntityBuilder = (TEntityBuilder) Activator.CreateInstance(typeof(TEntityBuilder), SetupTime)!;
            GrainId = ExistingEntityBuilder.Id;
            
            AddEntityBuilders(ExistingEntityBuilder);
        }

        protected override TGrain ConstructGrain()
        {
            _existingEntity = ExistingEntityBuilder.Object;
            return Silo.CreateGrainAsync<TGrain>(GrainId).GetAwaiter().GetResult();
        }
          
    }
}