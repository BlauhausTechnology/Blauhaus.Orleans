using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Responses;
using Blauhaus.SignalR.Abstractions.Auth;

namespace Blauhaus.SignalR.Abstractions
{
    public interface IConnectedCommandHandler<TResponse, TCommand, TUser>  
    {
        Task<Response<TResponse>> HandleRequestAsync(TCommand command, IConnectionContext context, Expression<Func<TCommand, TUser, Guid>> idResolver);
    }
}