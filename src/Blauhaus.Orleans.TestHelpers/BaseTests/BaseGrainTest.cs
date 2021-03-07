using System;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Auth.TestHelpers.MockBuilders;
using Blauhaus.Orleans.Abstractions.Streams;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.TestHelpers.MockBuilders;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.Builders._Base;
using Blauhaus.TestHelpers.MockBuilders;
using Blauhaus.Time.Abstractions;
using Blauhaus.Time.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using Orleans;
using Orleans.Core;
using Orleans.TestKit;
using Orleans.TestKit.Services;
using Orleans.TestKit.Streams;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{

    public abstract class BaseGrainTest<TSut, TId> : BaseServiceTest<TSut> 
        where TSut : Grain
    {
        protected TestKitSilo Silo;

        protected TId GrainId;
        
        protected DateTime SetupTime { get; private set; }
        protected DateTime RunTime { get; private set; }
        
        //Users
        protected ConnectedUserMockBuilder MockUser;
        protected AuthenticatedUserMockBuilder MockAdminUser;
        protected IAuthenticatedUser AdminUser => MockAdminUser.Object;
        protected IConnectedUser User => MockUser.Object;
        
        [SetUp]
        public void Setup()
        {
            base.Cleanup();

            Silo = new TestKitSilo();

            MockUser = new ConnectedUserMockBuilder();
            MockAdminUser = new AuthenticatedUserMockBuilder().With_Claim(new UserClaim("Role", "Admin"));

            //Infrastructure
            AddSiloService(MockTimeService.Object);
            AddSiloService(MockAnalyticsService.Object);

            SetupTime = MockTimeService.Reset();
            HandleSetup();
            RunTime = MockTimeService.AddSeconds(122);
        }
         
        protected abstract void HandleSetup();

        protected TimeServiceMockBuilder MockTimeService => AddMock<TimeServiceMockBuilder, ITimeService>().Invoke();
        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
        
        protected void AddSiloService<T>(T service) where T : class
        {
            Silo.ServiceProvider.AddService(service);
        } 
          
        protected Mock<T> AddMockGrain<T>(Guid grainKey) where T : class, IGrainWithGuidKey
        {
            return Silo.AddProbe<T>(grainKey);
        }

        protected void AddMockGrain<T>(Func<IGrainIdentity, IMock<T>> factory) where T : class, IGrain
        {
            Silo.AddProbe<T>(factory);
        }
        protected TMockBuilder AddMockGrain<TMock, TMockBuilder>() 
            where TMock : class, IGrain
            where TMockBuilder : BaseMockBuilder<TMockBuilder, TMock>, new()
        {
            var mockBuilder = new TMockBuilder();
            Silo.AddProbe(id => mockBuilder.Mock);
            return mockBuilder;
        }
        
        protected MockBuilder<TMock> AddMockGrain<TMock>()
            where TMock : class, IGrain
        {
            var mockBuilder = new MockBuilder<TMock>();
            Silo.AddProbe(_ => (IMock<TMock>) mockBuilder.Mock);
            return mockBuilder;
        }

        protected Mock<T> AddMockGrain<T>(string grainKey) where T : class, IGrainWithStringKey
        {
            return Silo.AddProbe<T>(grainKey);
        }

        protected TestStream<T> AddTestPersistentStream<T>(Guid id, string streamNamespace)
        {
            return Silo.StreamProviderManager.AddStreamProbe<T>(id, streamNamespace, StreamProvider.Persistent);
        }
        protected TestStream<T> AddTestTransientStream<T>(Guid id, string streamNamespace)
        {
            return Silo.StreamProviderManager.AddStreamProbe<T>(id, streamNamespace, StreamProvider.Transient);
        }

    }
 
}