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
using Blauhaus.Orleans.Resolver;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public class BaseActorGrainTest<TGrain, TModel, TModelBuilder, TActor, TActorMockBuilder, TDto, TDtoBuilder> : BaseActorGrainTest<TGrain, IGrainResolver, GrainResolverMockBuilder , TModel, TModelBuilder, TActor, TActorMockBuilder, TDto, TDtoBuilder>
        where TGrain : BaseActorGrain<TGrain, IGrainResolver, TActor, TModel, TDto>
        where TActor : class, IDtoModelActor<TModel, TDto, Guid>
        where TModel : class, IHasId<Guid>
        where TActorMockBuilder : BaseMockBuilder<TActorMockBuilder, TActor>, new()
        where TModelBuilder : class, IBuilder<TModelBuilder, TModel>, new()
        where TDto : IHasId<Guid>
        where TDtoBuilder : class, IBuilder<TDtoBuilder, TDto>, new()
    {

    }
    public class BaseActorGrainTest<TGrain, TGrainResolver, TGrainResolverMockBuilder , TModel, TModelBuilder, TActor, TActorMockBuilder, TDto, TDtoBuilder> 
        : BaseGuidGrainTest<TGrain> 
        where TGrain : BaseActorGrain<TGrain, TGrainResolver, TActor, TModel, TDto>
        where TActor : class, IDtoModelActor<TModel, TDto, Guid>
        where TModel : class, IHasId<Guid>
        where TActorMockBuilder : BaseMockBuilder<TActorMockBuilder, TActor>, new()
        where TModelBuilder : class, IBuilder<TModelBuilder, TModel>, new()
        where TDto : IHasId<Guid>
        where TGrainResolver : class, IGrainResolver
        where TGrainResolverMockBuilder : BaseGrainResolverMockBuilder<TGrainResolverMockBuilder, TGrainResolver>, new()
        where TDtoBuilder : class, IBuilder<TDtoBuilder, TDto>, new()
    {

        protected TActorMockBuilder MockActor = null!;
        protected TModelBuilder ModelBuilder = null!;
        protected TGrainResolverMockBuilder MockGrainResolver = null!;
        protected TDtoBuilder DtoBuilder = null!;

        public override void Setup()
        {
            base.Setup();

            GrainId = Guid.NewGuid();

            ModelBuilder = new TModelBuilder();
            ModelBuilder.With(x => x.Id, GrainId);

            DtoBuilder = new TDtoBuilder();
            DtoBuilder.With(x => x.Id, GrainId);

            MockActor = new TActorMockBuilder();
            MockActor.Mock.Setup(x => x.GetModelAsync()).ReturnsAsync(() => ModelBuilder.Object);
            MockActor.Mock.Setup(x => x.GetDtoAsync()).ReturnsAsync(() => DtoBuilder.Object);
            AddSiloService(MockActor.Object);

            MockGrainResolver = new TGrainResolverMockBuilder();
            AddSiloService(MockGrainResolver.Object);
        }
       
    }
}