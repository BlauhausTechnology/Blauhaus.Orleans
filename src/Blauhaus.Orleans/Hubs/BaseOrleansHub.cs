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


        # region Handle commands for Guid grains with return value

        protected Task<Response<TResponse>> HandleGuidGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, Guid> idResolver,
            string? messageTemplate = null, params object[] args)
                where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return LocalHandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        #endregion

        # region Handle void commands for Guid grains

        protected Task<Response> HandleGuidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, Guid> idResolver, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithGuidKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return LocalHandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        #endregion
        

        # region Handle commands for string grains with return value

        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, string> idResolver,
            string? messageTemplate = null, params object[] args)
                where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
                where TCommand : notnull
        {
            return LocalHandleCommandAsync(command, headers, idResolver, id
                => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        protected Task<Response<TResponse>> HandleStringGrainCommandAsync<TResponse, TIResponse, TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, string> idResolver,
            string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IAuthenticatedCommandHandler<TIResponse, TCommand, IConnectedUser>
            where TCommand : notnull
            where TResponse : TIResponse
        {
            return LocalHandleCommandAsync<TResponse, TIResponse, TCommand, string>(command, headers, idResolver, id 
                => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        #endregion

        # region Handle void commands for string grains

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, Func<TCommand, IConnectedUser, string> idResolver, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return LocalHandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }

        protected Task<Response> HandleVoidGrainCommandAsync<TCommand, TGrain>(
            TCommand command, Dictionary<string, object> headers, string grainId, string? messageTemplate = null, params object[] args)
            where TGrain : IGrainWithStringKey, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleVoidCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id), messageTemplate, args);
        }
         
        #endregion


                protected async Task<Response> LocalHandleCommandAsync<TCommand, TId>(
            TCommand command, 
            Dictionary<string, object> headers, 
            Func<TCommand, IConnectedUser, TId> idResolver,
            Func<TId, IVoidAuthenticatedCommandHandler<TCommand, IConnectedUser>> handlerResolver, 
            string? messageTemplate = null, params object[] args) 
                where TCommand : notnull
        {
            Logger.SetValues(headers);
            if (messageTemplate == null)
            {
                messageTemplate = "Hub handled command {CommandType}";
                args = new object[] { typeof(TCommand).Name };
            }

            using var _ = Logger.BeginTimedScope(LogLevel.Trace, messageTemplate, args);
            
            try
            {
                var connectedUser = GetConnectedUser();
                var id = idResolver.Invoke(command, connectedUser);
                var handler = handlerResolver.Invoke(id);

                Logger.SetValue("UserId", connectedUser.UserId);
                Logger.SetValue("ConnectionId", connectedUser.CurrentConnectionId);

                return await handler.HandleAsync(command, connectedUser);
            }
            catch (ErrorException error)
            {
                return Logger.LogErrorResponse(error.Error);
            }
            catch (Exception e)
            {
                return Logger.LogErrorResponse(Error.Unexpected(e.Message), e);
            }
        }

        protected async Task<Response<TResponse>> LocalHandleCommandAsync<TResponse, TCommand, TId>(
            TCommand command, 
            Dictionary<string, object> headers, 
            Func<TCommand, IConnectedUser, TId> idResolver,
            Func<TId, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>> handlerResolver, 
            string? messageTemplate = null, params object[] args) 
                where TCommand : notnull
        {
            Logger.SetValues(headers);
            if (messageTemplate == null)
            {
                messageTemplate = "Hub handled command {CommandType} for response {ResponseType}";
                args = new object[] { typeof(TCommand).Name, typeof(TResponse).Name };
            }

            using var _ = Logger.BeginTimedScope(LogLevel.Trace, messageTemplate, args);

            try
            {
                var connectedUser = GetConnectedUser();
                var id = idResolver.Invoke(command, connectedUser);
                var handler = handlerResolver.Invoke(id);

                return await handler.HandleAsync(command, connectedUser);
            }
            catch (ErrorException error)
            {
                return Logger.LogErrorResponse<TResponse>(error.Error);
            }
            catch (Exception e)
            {
                return Logger.LogErrorResponse<TResponse>(Error.Unexpected(e.Message), e);
            }
        } 
         

        //this is to allow internal return values to be IModel but still send a Model result to the client because SignalR doesn't support interfaces
        protected async Task<Response<TResponse>> LocalHandleCommandAsync<TResponse, TIResponse, TCommand, TId>(
            TCommand command, 
            Dictionary<string, object> headers, 
            Func<TCommand, IConnectedUser, TId> idResolver,
            Func<TId, IAuthenticatedCommandHandler<TIResponse, TCommand, IConnectedUser>> handlerResolver, 
            string? messageTemplate = null, params object[] args) 
                where TCommand : notnull
                where TResponse : TIResponse
        {
            Logger.SetValues(headers);
            if (messageTemplate == null)
            {
                messageTemplate = "Hub handled command {CommandType} for response {ResponseType}";
                args = new object[] { typeof(TCommand).Name, typeof(TIResponse).Name };
            }

            using var _ = Logger.BeginTimedScope(LogLevel.Trace, messageTemplate, args);

            try
            {
                var connectedUser = GetConnectedUser();
                var id = idResolver.Invoke(command, connectedUser);
                var handler = handlerResolver.Invoke(id);

                var response = await handler.HandleAsync(command, connectedUser);
                return response.IsSuccess 
                    ? Response.Success((TResponse)response.Value!) 
                    : Response.Failure<TResponse>(response.Error);
            }
            catch (ErrorException error)
            {
                return Logger.LogErrorResponse<TResponse>(error.Error);
            }
            catch (Exception e)
            {
                return Logger.LogErrorResponse<TResponse>(Error.Unexpected(e.Message), e);
            }
        } 
         
    }
}