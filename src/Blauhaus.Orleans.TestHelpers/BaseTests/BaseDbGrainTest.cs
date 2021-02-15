using System;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.Orleans.EfCore.Grains;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseDbGrainTest<TSut, TDbContext> : BaseIdGrainTest<TSut> where TSut : BaseDbGrain<TDbContext> where TDbContext : DbContext
    {
        protected sealed override void HandleSetup()
        {
            base.HandleSetup();
            
            var dbContextBuilder = new InMemoryDbContextBuilder<TDbContext>();

            TDbContext FactoryFunc() => dbContextBuilder.NewContext;
            AddSiloService((Func<TDbContext>) FactoryFunc);

            using (var setupContext = dbContextBuilder.NewContext)
            {
                //a different context for test setup
                SetupDbContext(setupContext);
                setupContext.SaveChanges();
            }

            //and a different one for test assertions
            PostDbContext = dbContextBuilder.NewContext;
        }

        protected abstract void SetupDbContext(TDbContext setupContext);
        
        protected TDbContext PostDbContext;
    }
}