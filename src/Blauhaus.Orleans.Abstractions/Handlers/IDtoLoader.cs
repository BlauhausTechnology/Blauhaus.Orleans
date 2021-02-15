using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Orleans.Abstractions.Handlers
{
    public interface IDtoLoader<TDto> where TDto : ISyncClientEntity
    {
        Task<TDto> GetDtoAsync();
    }

    
    public interface IDtosLoader<TDto> where TDto : ISyncClientEntity
    {
        Task<List<TDto>> GetDtosAsync();
    }
}