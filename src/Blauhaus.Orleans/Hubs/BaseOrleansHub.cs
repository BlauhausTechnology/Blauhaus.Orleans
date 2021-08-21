using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Services;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Errors;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Responses;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Abstractions.Server.Handlers;
using Blauhaus.SignalR.Abstractions.Sync;
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

        protected Task<Response<TResponse>> HandleGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Expression<Func<TCommand, IConnectedUser, Guid>> idResolver)
                where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
        {
            return HandleCommandAsync(command, headers, idResolver, id => ClusterClient.GetGrain<TGrain>(id));
        }

        protected Task<Response<TResponse>> HandleGrainCommandAsync<TResponse, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers, Guid grainId)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>
        {
            return HandleCommandAsync(command, headers, (x,y) => grainId, id => ClusterClient.GetGrain<TGrain>(id));
        }
         
        protected Task<Response<TDto>> HandleGrainCommandForUserAsync<TDto, TCommand, TGrain>(
            TCommand command, IDictionary<string, string> headers)
            where TGrain : IGrainWithGuidKey, IAuthenticatedCommandHandler<TDto, TCommand, IConnectedUser>
            where TCommand : notnull
        {
            return HandleCommandAsync(command, headers, (comm, user) => user.UserId, id => ClusterClient.GetGrain<TGrain>(id));
        }

        protected async Task<Response<SyncResponse<TDto>>> HandleSyncRequestForUserAsync<TDto, TGrain>(SyncRequest command, IDictionary<string, string> headers)
            where TGrain : IGrainWithGuidKey, ISyncRequestHandler<TDto> where TDto : IClientEntity
        {
            using (var _ = AnalyticsService.StartRequestOperation(this, $"Sync {typeof(TDto).Name}", headers))
            {
                try
                {
                    var connectedUser = GetConnectedUser();
                    var id = connectedUser.UserId;
                    var publisherGrain = ClusterClient.GetGrain<TGrain>(id);
                    return await publisherGrain.HandleAsync(command, connectedUser);
                }
                catch (ErrorException error)
                {
                    return AnalyticsService.TraceErrorResponse<SyncResponse<TDto>>(this, error.Error, command.ToObjectDictionary());
                }
                catch (Exception e)
                {
                    return AnalyticsService.LogExceptionResponse<SyncResponse<TDto>>(this, e, Errors.Errors.Unexpected(e.Message), command.ToObjectDictionary());
                }
            }
        }
    }
}