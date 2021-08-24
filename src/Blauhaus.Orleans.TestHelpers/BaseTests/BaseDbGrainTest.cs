﻿using System;
using System.Threading.Tasks;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
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
        
        protected override void HandleSetup()
        {
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
        
        protected TDbContext PostDbContext = null!;

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