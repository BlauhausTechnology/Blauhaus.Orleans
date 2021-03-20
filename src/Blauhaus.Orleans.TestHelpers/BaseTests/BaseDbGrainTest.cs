using System;
using System.Threading.Tasks;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
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

        protected void Seed<T>(T entity)
        {
            AddtionalSetup(context =>
            {
                context.Seed(entity);
            });
        }
        
        protected void AddtionalSetup(Action<TDbContext> setupFunc)
        {
            using (var dbContext =  _dbContextBuilder.NewContext)
            {
                setupFunc.Invoke(dbContext);
                dbContext.SaveChanges();
            }
        }
    }
}