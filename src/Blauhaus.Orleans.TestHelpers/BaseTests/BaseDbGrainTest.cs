﻿using System;
using System.Collections.Generic;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.TestHelpers.Builders.Base;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
     
    public abstract class BaseDbGrainTest<TGrain, TDbContext, TId, TGrainResolver> : BaseGrainTest<TGrain, TId> 
        where TGrain : BaseDbGrain<TDbContext, TGrainResolver>
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
            _dbContextBefore = GetNewDbContext();

            SetupTime = MockTimeService.Reset();
            RunTime = SetupTime.AddSeconds(122);

            TDbContext FactoryFunc() => GetNewDbContext();
            AddSiloService((Func<TDbContext>) FactoryFunc);

        }
         
       
        protected sealed override TGrain ConstructSut()
        {
            foreach (var setupFunc in _entityFactories)
            {
                setupFunc.Invoke(DbContextBefore);
            }
            DbContextBefore.SaveChanges();
            _dbContextBefore = null;

            MockTimeService.With(x => x.CurrentUtcTime, RunTime);

            var sut = ConstructGrain();
            
            _dbContextAfter = GetNewDbContext();

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
            using (var dbContext =  GetNewDbContext())
            {
                setupFunc.Invoke(dbContext);
                dbContext.SaveChanges();
            }
        }

        protected TDbContext GetNewDbContext() => _dbContextBuilder.NewContext;
    }
}