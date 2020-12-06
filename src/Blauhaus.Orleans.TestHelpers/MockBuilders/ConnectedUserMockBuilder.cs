using System;
using System.Collections.Generic;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Orleans.TestHelpers.MockBuilders
{
    public class ConnectedUserMockBuilder : BaseMockBuilder<ConnectedUserMockBuilder, IConnectedUser>
    {
        public ConnectedUserMockBuilder()
        {
            With(x => x.EmailAddress, Guid.NewGuid() + "@freever.com");
            With(x => x.UserId, Guid.NewGuid());
            With(x => x.Claims, new List<UserClaim>());
            With(x => x.CurrentConnectionId, Guid.NewGuid().ToString());
            With(x => x.UserId, Guid.NewGuid());
        }

        public ConnectedUserMockBuilder With_Claim(UserClaim claim)
        {
            Mock.Setup(x => x.HasClaim(claim.Name)).Returns(true);
            Mock.Setup(x => x.HasClaimValue(claim.Name, claim.Value)).Returns(true);
            With(x => x.Claims, new List<UserClaim>{claim});
            return this;
        }
    }
}