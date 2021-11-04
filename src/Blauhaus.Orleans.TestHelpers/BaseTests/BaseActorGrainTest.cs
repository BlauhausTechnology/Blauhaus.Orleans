using Blauhaus.TestHelpers.Builders.Base;
using Blauhaus.TestHelpers.MockBuilders;
using Orleans;
using System;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Actors;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.Grains;
using Blauhaus.Orleans.TestHelpers.MockBuilders.Resolver;
using Moq;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public class BaseActorGrainTest<TGrainResolver, TGrainResolverMockBuilder, TSut, TModel, TModelBuilder, TActor, TActorMockBuilder, TDto> : BaseGuidGrainTest<TSut> 
        where TSut : BaseActorGrain<TGrainResolver, TActor, TModel, TDto>, IGrainWithGuidKey
        where TActor : class, IDtoModelActor<TModel, TDto, Guid>
        where TModel : class, IHasId<Guid>
        where TActorMockBuilder : BaseMockBuilder<TActorMockBuilder, TActor>, new()
        where TModelBuilder : class, IBuilder<TModelBuilder, TModel>, new()
        where TDto : IHasId<Guid>
        where TGrainResolver : class, IGrainResolver
        where TGrainResolverMockBuilder : BaseGrainResolverMockBuilder<TGrainResolverMockBuilder, TGrainResolver>
    {

        protected TActorMockBuilder MockActor = null!;
        protected TModelBuilder ModelBuilder = null!;

        public override void Setup()
        {
            base.Setup();

            GrainId = Guid.NewGuid();
            ModelBuilder = new TModelBuilder();
            ModelBuilder.With(x => x.Id, GrainId);

            MockActor = new TActorMockBuilder();
            MockActor.Mock.Setup(x => x.GetModelAsync()).ReturnsAsync(() => ModelBuilder.Object);
            AddSiloService(MockActor.Object);

            AddSiloService(MockGrainResolver.Object);
        }


        protected TGrainResolverMockBuilder MockGrainResolver = null!;
       
    }
}