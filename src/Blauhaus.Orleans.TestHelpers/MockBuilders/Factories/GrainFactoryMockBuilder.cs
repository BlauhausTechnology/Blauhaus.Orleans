using System;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.MockBuilders.Factories
{

    public abstract class BaseGrainFactoryMockBuilder<TBuilder, TMock> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IGrainFactory
        where TBuilder : BaseGrainFactoryMockBuilder<TBuilder, TMock>
    {
        public TBuilder Where_GetGuidGrain_returns<TGrain>(TGrain grain) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetGuidGrain_returns<TGrain>(TGrain grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(id, It.IsAny<string>())).Returns(grain);
            return (TBuilder) this;
        }


        public TBuilder Where_GetLongGrain_returns<TGrain>(TGrain grain) where TGrain : IGrainWithIntegerKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<long>(), It.IsAny<string>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetLongGrain_returns<TGrain>(TGrain grain, long id) where TGrain : IGrainWithIntegerKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(id, It.IsAny<string>())).Returns(grain);
            return (TBuilder) this;
        }


        public TBuilder Where_GetStringGrain_returns<TGrain>(TGrain grain) where TGrain : IGrainWithStringKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<string>(), It.IsAny<string>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetStringGrain_returns<TGrain>(TGrain grain, string id) where TGrain : IGrainWithStringKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(id, It.IsAny<string>())).Returns(grain);
            return (TBuilder) this;
        }

    }

    public class GrainFactoryMockBuilder : BaseGrainFactoryMockBuilder<GrainFactoryMockBuilder, IGrainFactory>
    {
        
    }
}