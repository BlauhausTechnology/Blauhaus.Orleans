using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Orleans.Abstractions.Handlers
{
    public interface IDtoLoader<TDto> where TDto : IClientEntity
    {
        Task<TDto> GetDtoAsync();
    }

    
    public interface IDtosLoader<TDto> where TDto : IClientEntity
    {
        Task<List<TDto>> GetDtosAsync();
    }
}