using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.TestHelpers.Builders.Base;
using Microsoft.EntityFrameworkCore;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
     
    public abstract class BaseDbGrainTest<TSut, TDbContext, TId, TGrainResolver> : BaseGrainTest<TSut, TId> 
        where TSut : BaseDbGrain<TDbContext, TGrainResolver>
        where TDbContext : DbContext
        where TGrainResolver : IGrainResolver
    {
        private InMemoryDbContextBuilder<TDbContext> _dbContextBuilder = null!;
     
        private readonly List<Action<TDbContext>> _entityFactories = new();   

        protected DateTime SetupTime;
        protected DateTime RunTime;

        private TDbContext? _dbContextBefore;
        protected TDbContext DbContextBefore
        {
            get
            {
                if (_dbContextBefore == null)
                {
                    throw new InvalidOperationException("DbContextBefore is no longer valid once the test has started running");
                }

                return _dbContextBefore;
            }
        }

        private TDbContext? _dbContextAfter;
        protected TDbContext DbContextAfter
        {
            get
            {
                if (_dbContextAfter == null)
                {
                    throw new InvalidOperationException("DbContextAfter is not valid until the test has finished running");
                }

                return _dbContextAfter;
            }
        }

        protected void AddEntityBuilders<T>(params IBuilder<T>[] builders) where T : BaseServerEntity 
        {
            foreach (var builder in builders)
            {
                _entityFactories.Add(context=> context.Add(builder.Object));
            } 
        }
        protected void AddEntityBuilder<T>(IBuilder<T> builder) where T : BaseServerEntity 
        {
            _entityFactories.Add(context=> context.Add(builder.Object));
        }

        protected void AddEntityBuilders<T>(List<IBuilder<T>> builders) where T : BaseServerEntity 
        {
            foreach (var builder in builders)
            {
                _entityFactories.Add(context=> context.Add(builder.Object));
            } 
        }

        public override void Setup()
        {
            base.Setup();

            _entityFactories.Clear();
            _dbContextBuilder = new InMemoryDbContextBuilder<TDbContext>();

            _dbContextAfter = null;
            _dbContextBefore = _dbContextBuilder.NewContext;

            SetupTime = MockTimeService.Reset();
            RunTime = SetupTime.AddSeconds(122);

            TDbContext FactoryFunc() => _dbContextBuilder.NewContext;
            AddSiloService((Func<TDbContext>) FactoryFunc);

        }
         
       
        protected sealed override TSut ConstructSut()
        {
            foreach (var setupFunc in _entityFactories)
            {
                setupFunc.Invoke(DbContextBefore);
            }
            DbContextBefore.SaveChanges();
            _dbContextBefore = null;

            MockTimeService.With(x => x.CurrentUtcTime, RunTime);

            var sut = ConstructGrain();
            
            _dbContextAfter = _dbContextBuilder.NewContext;

            return sut;
        }
         

        protected T Seed<T>(T entity)
        {
            AdditionalSetup(context =>
            {
                context.Seed(entity);
            });
            return entity;
        }
        
        protected void AdditionalSetup(Action<TDbContext> setupFunc)
        {
            using (var dbContext =  _dbContextBuilder.NewContext)
            {
                setupFunc.Invoke(dbContext);
                dbContext.SaveChanges();
            }
        }
    }
}