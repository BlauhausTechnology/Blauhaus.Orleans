using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Actors;
using Blauhaus.Orleans.Abstractions.Grains;
using Blauhaus.Orleans.Abstractions.Resolver;
using Orleans;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseActorGrain<TGrainResolver, TActor, TModel, TDto> : BaseActorGrain<TGrainResolver, TActor, TModel>, IActorGrain<TModel, TDto>
        where TModel : IHasId<Guid>
        where TActor : IDtoModelActor<TModel, TDto, Guid>
        where TDto : IHasId<Guid>
        where TGrainResolver : IGrainResolver
    {
        protected BaseActorGrain(
            IAnalyticsService analyticsService, 
            TGrainResolver grainResolver, 
            TActor actor) 
                : base(analyticsService, grainResolver, actor)
        {
        }

        public Task<TDto> GetDtoAsync()
        {
            return Actor.GetDtoAsync();
        }
    }

    public abstract class BaseActorGrain<TGrainResolver, TActor, TModel> : BaseResolverGrain<TGrainResolver>, IActorGrain<TModel>
        where TModel : IHasId<Guid>
        where TActor : IModelActor<TModel, Guid>
        where TGrainResolver : IGrainResolver
    {
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly TActor Actor;
        protected Guid Id;

        protected BaseActorGrain(
            IAnalyticsService analyticsService, 
            TGrainResolver grainResolver,
            TActor actor) 
            : base(grainResolver)
        {
            AnalyticsService = analyticsService;
            Actor = actor;
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                await base.OnActivateAsync();

                Id = this.GetPrimaryKey();
                await Actor.InitializeAsync(Id);

                if (Id == Guid.Empty)
                {
                    throw new ArgumentException($"Grain requires a GUID id. \"{this.GetPrimaryKey()}\" is not valid");
                }
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
                throw;
            }
        }

        public Task<TModel> GetModelAsync()
        {
            return Actor.GetModelAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            await Actor.DisposeAsync();
        }
    }
}