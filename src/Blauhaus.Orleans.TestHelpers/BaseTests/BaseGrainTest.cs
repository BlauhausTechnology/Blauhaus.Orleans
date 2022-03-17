using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Auth.TestHelpers.MockBuilders;
using Blauhaus.Orleans.Abstractions.Streams;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.TestHelpers.MockBuilders;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.MockBuilders;
using Blauhaus.Time.Abstractions;
using Blauhaus.Time.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using Orleans;
using Orleans.Core;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{

    public abstract class BaseGrainTest<TSut, TId> : BaseServiceTest<TSut> 
        where TSut : Grain
    {
        protected TestKitSilo Silo = null!;

        protected TId GrainId = default!;

        
        //Users
        protected ConnectedUserMockBuilder MockUser = null!;
        protected AuthenticatedUserMockBuilder MockAdminUser = null!;
        protected IAuthenticatedUser AdminUser => MockAdminUser.Object;
        protected IConnectedUser User => MockUser.Object;
        
        [SetUp]
        public virtual void Setup()
        {
            base.Cleanup();

            Silo = new TestKitSilo();

            MockUser = new ConnectedUserMockBuilder();
            MockAdminUser = new AuthenticatedUserMockBuilder().With_Claim(new UserClaim("Role", "Admin"));

            //Infrastructure
            AddSiloService(MockTimeService.Object);
            AddSiloService(MockAnalyticsService.Object);
        }
          

        protected TimeServiceMockBuilder MockTimeService => AddMock<TimeServiceMockBuilder, ITimeService>().Invoke();
        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();

        protected override TSut ConstructSut()
        {
            var provider = Silo.ServiceProvider;
            BeforeConstructSut(provider);
            var sut = ConstructGrain();
            Task.Run(async () => await AfterConstructSutAsync(sut)).Wait();
            return sut;
        }

        protected abstract TSut ConstructGrain();

        protected void AddSiloService<T>(T service) where T : class
        {
            Silo.ServiceProvider.AddService(service);
        } 
          
        protected void AddMockGrain<T>(Func<IGrainIdentity, IMock<T>> factory) where T : class, IGrain
        {
            Silo.AddProbe<T>(factory);
        }
        protected TMockBuilder AddMockGrain<TGrain, TMockBuilder>() 
            where TGrain : class, IGrain
            where TMockBuilder : BaseMockBuilder<TMockBuilder, TGrain>, new()
        {
            var mockBuilder = new TMockBuilder();
            Silo.AddProbe(id => mockBuilder.Mock);
            return mockBuilder;
        }
        
        protected MockBuilder<TGrain> AddMockGrain<TGrain>()
            where TGrain : class, IGrain
        {
            var mockBuilder = new MockBuilder<TGrain>();
            Silo.AddProbe(_ => (IMock<TGrain>) mockBuilder.Mock);
            return mockBuilder;
        } 
        
        protected Mock<TGrain> AddMockGrain<TGrain>(string grainKey) where TGrain : class, IGrainWithStringKey
        {
            return Silo.AddProbe<TGrain>(grainKey);
        }
        
        protected Mock<TGrain> AddMockGrain<TGrain>(Guid grainKey) where TGrain : class, IGrainWithGuidKey
        {
            return Silo.AddProbe<TGrain>(grainKey);
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