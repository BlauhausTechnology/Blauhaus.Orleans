using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Responses;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Server.Auth;
using Blauhaus.SignalR.Server.Hubs;
using Microsoft.Extensions.Logging;
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

        
        protected Task<Response> HandleGuidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, Guid> idResolver, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response<TResponse>> HandleGuidGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, Guid> idResolver,
            string? messageTemplate = null, params object[] args)
                where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response> HandleStringGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, string> idResolver, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, string> idResolver,
            string? messageTemplate = null, params object[] args)
                where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, idResolver, id
                => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TIResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, string> idResolver,
            string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TIResponse, TCommand, IConnectedUser>
            where TCommand : notnull
            where TResponse : TIResponse
        {
            return HandleCommandAsync<TResponse, TIResponse, TCommand, string>(command, headers, idResolver, id 
                => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }



    }
}