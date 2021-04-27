using System;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.MockBuilders.Resolver
{
    public abstract class BaseGrainResolverMockBuilder<TBuilder, TMock> : BaseMockBuilder<TBuilder, TMock>
        where TBuilder : BaseGrainResolverMockBuilder<TBuilder, TMock>
        where TMock : class, IGrainResolver
    {
        public TBuilder Where_GetGrain_returns<TGrain>(TGrain grain, Guid? id = null) where TGrain : IGrainWithGuidKey
        {
            if (id == null)
            {
                Mock.Setup(x => x.GetGrain<TGrain>(It.IsAny<Guid>())).Returns(grain);
            }
            else
            {
                Mock.Setup(x => x.GetGrain<TGrain>(id.Value)).Returns(grain);
            }
            return (TBuilder) this;
        }
    }
}