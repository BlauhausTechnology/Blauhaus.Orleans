using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Responses;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Server.Auth;
using Blauhaus.SignalR.Server.Hubs;
using Orleans;

namespace Blauhaus.Orleans.Hubs
{
    public abstract class BaseOrleansHub : BaseSignalRHub
    {
        protected readonly IClusterClient ClusterClient;

        protected BaseOrleansHub(
            IServiceLocator serviceLocator, 
            IAnalyticsService analyticsService, 
            IConnectedUserFactory connectedUserFactory,
            IClusterClient clusterClient) 
                : base(serviceLocator, analyticsService, connectedUserFactory)
        {
            ClusterClient = clusterClient;
        }


        //Handle commands with return value

        protected Task<Response<TResponse>> HandleGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Expression<Func<TCommand, IConnectedUser, Guid>> idResolver)
                where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id));
        }

        protected Task<Response<TResponse>> HandleGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Guid grainId)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id));
        }
         
        protected Task<Response<TResponse>> HandleGrainCommandForUserAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, (comm, user) => user.UserId, id => ClusterClient.GetGrain<TGrain>(id));
        }


        
        //Handle void commands 
       
        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Expression<Func<TCommand, IConnectedUser, Guid>> idResolver)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id));
        }

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Guid grainId)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id));
        }

        protected Task<Response> HandleVoidGrainCommandForUserAsync<TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, (comm, user) => user.UserId, id => ClusterClient.GetGrain<TGrain>(id));
        }


    }
}