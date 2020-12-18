using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Services;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Responses;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Server.Hubs;
using Orleans;

namespace Blauhaus.Orleans
{
    public class BaseOrleansHub : BaseSignalrHub
    {
        protected readonly IClusterClient ClusterClient;

        public BaseOrleansHub(
            IServiceLocator serviceLocator, 
            IAnalyticsService analyticsService, 
            IAuthenticatedUserFactory authenticatedUserFactory,
            IClusterClient clusterClient) 
                : base(serviceLocator, analyticsService, authenticatedUserFactory)
        {
            ClusterClient = clusterClient;
        }

        protected Task<Response<TResponse>> HandleGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Expression<Func<TCommand, IConnectedUser, Guid>> idResolver)
                where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id));
        }

        protected Task<Response<TResponse>> HandleGrainCommandForUserAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
        {
            return HandleCommandAsync(command, headers, (comm, user) => user.UserId, id => ClusterClient.GetGrain<TGrain>(id));
        }

    }
}