using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
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
            IAnalyticsLogger logger, 
            IServiceLocator serviceLocator, 
            IConnectedUserFactory connectedUserFactory,
            IClusterClient clusterClient) 
                : base(logger, serviceLocator, connectedUserFactory)
        {
            ClusterClient = clusterClient;
        }


        # region Handle commands for Guid grains with return value

        protected Task<Response<TResponse>> HandleGuidGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Expression<Func<TCommand, IConnectedUser, Guid>> idResolver,
            string? messageTemplate = null, params object[] args)
                where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response<TResponse>> HandleGuidGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Guid grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        protected Task<Response<TResponse>> HandleGuidGrainCommandAsync<TResponse, TIResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Guid grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TIResponse, TCommand, IConnectedUser>
            where TCommand : notnull
            where TResponse : TIResponse
        {
            return HandleCommandAsync<TResponse, TIResponse, TCommand, Guid>(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        protected Task<Response<TResponse>> HandleGuidGrainCommandForUserAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, (comm, user) => user.UserId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        #endregion

        # region Handle void commands for Guid grains

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Expression<Func<TCommand, IConnectedUser, Guid>> idResolver, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Guid grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response> HandleVoidGrainCommandForUserAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, (comm, user) => user.UserId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        #endregion
        

        # region Handle commands for string grains with return value

        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Expression<Func<TCommand, IConnectedUser, string>> idResolver,
            string? messageTemplate = null, params object[] args)
                where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, string grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
        
        
        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TIResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, string grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TIResponse, TCommand, IConnectedUser>
            where TCommand : notnull
            where TResponse : TIResponse
        {
            return HandleCommandAsync<TResponse, TIResponse, TCommand, string>(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        #endregion

        # region Handle void commands for string grains

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Expression<Func<TCommand, IConnectedUser, string>> idResolver, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, string grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        #endregion
    }
}