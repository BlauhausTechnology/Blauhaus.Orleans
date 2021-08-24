using System.Threading.Tasks;
using Blauhaus.SignalR.Abstractions.Auth;
using Orleans.Concurrency;

namespace Blauhaus.Orleans.Abstractions.Handlers
{
    public interface IConnectedUserHandler
    {
        [OneWay]
        Task ConnectUserAsync(IConnectedUser user);
        
        [OneWay]
        Task DisconnectUserAsync(IConnectedUser user);
    }
}