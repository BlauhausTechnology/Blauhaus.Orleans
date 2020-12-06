using Blauhaus.Auth.Abstractions.User;
using Blauhaus.SignalR.Abstractions.Auth;

namespace Blauhaus.SignalR.Server.Auth
{
    public class ConnectedUser : AuthenticatedUser, IConnectedUser
    {
        public ConnectedUser(
            IAuthenticatedUser authenticatedUser, 
            string currentDeviceIdentifier, 
            string currentConnectionId) 
                : base(authenticatedUser)
        {
            CurrentDeviceIdentifier = currentDeviceIdentifier;
            CurrentConnectionId = currentConnectionId;
        }

        public string CurrentDeviceIdentifier { get; }
        public string CurrentConnectionId { get; }
    }
}