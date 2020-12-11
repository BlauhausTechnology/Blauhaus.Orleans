using System;
using Blauhaus.TestHelpers.BaseTests;
using Moq;
using Orleans;
using Orleans.TestKit;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{

    public abstract class BaseGrainTest<TSut, TId> : BaseServiceTest<TSut> where TSut : Grain
    {
        protected TestKitSilo TestSilo;

        protected TId GrainId;
        
        protected DateTime SetupTime { get; private set; }
        protected DateTime RunTime { get; private set; }

        
        protected void AddSiloService<T>(T service) where T : class
        {
            TestSilo.ServiceProvider.AddService(service);
        }
        

    }

    public abstract class BaseStringGrainTest<TSut> : BaseGrainTest<TSut, string> where TSut : Grain, IGrainWithStringKey
    {
        
        protected Mock<T> AddMockGrain<T>(string grainKey) where T : class, IGrainWithStringKey
        {
            return TestSilo.AddProbe<T>(grainKey);
        }
        
        protected override TSut ConstructSut()
        {
            return TestSilo.CreateGrainAsync<TSut>(GrainId).GetAwaiter().GetResult();
        }
    }
}