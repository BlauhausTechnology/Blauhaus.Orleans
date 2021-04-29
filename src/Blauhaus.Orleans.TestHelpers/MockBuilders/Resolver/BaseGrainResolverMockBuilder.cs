using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.TestHelpers.Builders.Base;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.MockBuilders.Resolver
{
    public abstract class BaseGrainResolverMockBuilder<TBuilder, TMock> : BaseMockBuilder<TBuilder, TMock>
        where TBuilder : BaseGrainResolverMockBuilder<TBuilder, TMock>
        where TMock : class, IGrainResolver
    {
        public TBuilder Where_GetGrain_returns<TGrain>(TGrain grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(id)).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetGrain_returns<TGrain>(Func<TGrain> grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(id)).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetGrain_returns<TGrain>(IBuilder<TGrain> grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(id)).Returns(()=> grain.Object);
            return (TBuilder) this;
        }

        public TBuilder Where_GetGrain_returns<TGrain>(TGrain grain) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<Guid>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetGrain_returns<TGrain>(Func<TGrain> grain) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<Guid>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_GetGrain_returns<TGrain>(IBuilder<TGrain> grain) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<Guid>())).Returns(()=> grain.Object);
            return (TBuilder) this;
        }
    }
}