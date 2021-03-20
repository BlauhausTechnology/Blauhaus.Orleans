using System;
using System.Threading.Tasks;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.Orleans.EfCore.Grains;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseDbGrainTest<TSut, TDbContext> : BaseGuidGrainTest<TSut> where TSut : BaseDbGrain<TDbContext> where TDbContext : DbContext
    {
        private InMemoryDbContextBuilder<TDbContext> _dbContextBuilder;
        
        protected sealed override void HandleSetup()
        {
            base.HandleSetup();
            
            _dbContextBuilder = new InMemoryDbContextBuilder<TDbContext>();

            TDbContext FactoryFunc() => _dbContextBuilder.NewContext;
            AddSiloService((Func<TDbContext>) FactoryFunc);

            using (var setupContext = _dbContextBuilder.NewContext)
            {
                //a different context for test setup
                SetupDbContext(setupContext);
                setupContext.SaveChanges();
            }

            //and a different one for test assertions
            PostDbContext = _dbContextBuilder.NewContext;
        }

        protected abstract void SetupDbContext(TDbContext setupContext);
        
        protected TDbContext PostDbContext;
        protected TDbContext GetNewContext() => _dbContextBuilder.NewContext;

        protected async Task AddtionalSetupAsync(Func<TDbContext, Task> setupFunc)
        {
            using (var dbContext =  _dbContextBuilder.NewContext)
            {
                await setupFunc.Invoke(dbContext);
                
                await dbContext.SaveChangesAsync();
            }
        }
    }
}