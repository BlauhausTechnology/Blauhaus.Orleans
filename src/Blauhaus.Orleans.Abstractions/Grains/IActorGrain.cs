using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Orleans;
using System;

namespace Blauhaus.Orleans.Abstractions.Grains
{
    public interface IActorGrain<TModel, TDto> : IActorGrain<TModel>, IDtoOwner<TDto>
        where TModel : IHasId<Guid>
    {
        
    }

    public interface IActorGrain<TModel> : IGrainWithGuidKey, IModelOwner<TModel> 
        where TModel : IHasId<Guid>
    {
        
    }
}