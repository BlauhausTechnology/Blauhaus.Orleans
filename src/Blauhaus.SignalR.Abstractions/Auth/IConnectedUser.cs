using Blauhaus.Auth.Abstractions.User;

namespace Blauhaus.SignalR.Abstractions.Auth
{
    public interface IConnectedUser : IAuthenticatedUser
    {
        string CurrentDeviceIdentifier { get; }
        string CurrentConnectionId { get; }
    }
}