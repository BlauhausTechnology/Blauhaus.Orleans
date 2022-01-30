using System;
using Blauhaus.Orleans.Abstractions.Grains;
using Blauhaus.Orleans.Abstractions.Identity;
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

        public TBuilder Where_Resolve_returns<TGrain>(TGrain grain, GrainId grainId) where TGrain : class, IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(grainId)).Returns(grain);
            return (TBuilder) this;
        }


        public TBuilder Where_Resolve_returns<TGrain>(TGrain grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(id)).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_Resolve_returns<TGrain>(Func<TGrain> grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(id)).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_Resolve_returns<TGrain>(IBuilder<TGrain> grain, Guid id) where TGrain : IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(id)).Returns(()=> grain.Object);
            return (TBuilder) this;
        }

        public TBuilder Where_Resolve_returns<TGrain>(TGrain grain) where TGrain : class, IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(It.IsAny<Guid>())).Returns(grain);
            Mock.Setup(x => x.Resolve<TGrain>(It.IsAny<GrainId>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_Resolve_returns<TGrain>(Func<TGrain> grain) where TGrain : class, IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(It.IsAny<Guid>())).Returns(grain);
            Mock.Setup(x => x.Resolve<TGrain>(It.IsAny<GrainId>())).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_Resolve_returns<TGrain>(IBuilder<TGrain> grain) where TGrain : class, IGrainWithGuidKey
        {
            Mock.Setup(x => x.Resolve<TGrain>(It.IsAny<Guid>())).Returns(()=> grain.Object);
            Mock.Setup(x => x.Resolve<TGrain>(It.IsAny<GrainId>())).Returns(()=> grain.Object);
            return (TBuilder) this;
        }
        
        public TBuilder Where_ResolveSingleton_returns<TGrain>(TGrain grain) where TGrain : IGrainSingleton
        {
            Mock.Setup(x => x.ResolveSingleton<TGrain>()).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_ResolveSingleton_returns<TGrain>(Func<TGrain> grain) where TGrain : IGrainSingleton
        {
            Mock.Setup(x => x.ResolveSingleton<TGrain>()).Returns(grain);
            return (TBuilder) this;
        }
        public TBuilder Where_ResolveSingleton_returns<TGrain>(IBuilder<TGrain> grain) where TGrain : IGrainSingleton
        {
            Mock.Setup(x => x.ResolveSingleton<TGrain>()).Returns(()=> grain.Object);
            return (TBuilder) this;
        }

        public void VerifyResolve<TGrain>(Guid id, int? times =null) where TGrain : IGrainWithGuidKey
        {
            if (times == null)
            {
                Mock.Verify(x => x.Resolve<TGrain>(id));
            }
            else
            {
                Mock.Verify(x => x.Resolve<TGrain>(id), Times.Exactly(times.Value));
            }
        }
    }
}