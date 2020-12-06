using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Services;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Responses;
using Blauhaus.SignalR.Abstractions.Auth;
using Blauhaus.SignalR.Server.Auth;
using Microsoft.AspNetCore.SignalR;

namespace Blauhaus.SignalR.Server.Hubs
{
    public abstract class BaseSignalrHub : Hub
    {
        protected readonly IServiceLocator ServiceLocator;
        private readonly IAnalyticsService _analyticsService;
        private readonly IAuthenticatedUserFactory _authenticatedUserFactory; 

        protected BaseSignalrHub(
            IServiceLocator serviceLocator, 
            IAnalyticsService analyticsService,
            IAuthenticatedUserFactory authenticatedUserFactory)
        {
            ServiceLocator = serviceLocator;
            _analyticsService = analyticsService;
            _authenticatedUserFactory = authenticatedUserFactory; 
        }

        protected async Task<Response<TResponse>> HandleCommandAsync<TResponse, TCommand>(
            TCommand command, 
            IDictionary<string, string> headers, 
            Expression<Func<TCommand, IConnectedUser, Guid>> idResolver,
            Func<Guid, IAuthenticatedCommandHandler<TResponse, TCommand, IConnectedUser>> handlerResolver)  
        {
            using (var _ = _analyticsService.StartRequestOperation(this, typeof(TCommand).Name, headers))
            {
                try
                {
                    var deviceIdentifier = Context.GetHttpContext().Request.Query["device"];

                    var getUserResult = _authenticatedUserFactory.ExtractFromClaimsPrincipal(Context.User);
                    if (getUserResult.IsFailure) return Response.Failure<TResponse>(getUserResult.Error);

                    var authenticatedUser = getUserResult.Value;
                    var currentUser = new ConnectedUser(authenticatedUser, deviceIdentifier, Context.ConnectionId);
                    var id = idResolver.Compile().Invoke(command, currentUser);

                    var handler = handlerResolver.Invoke(id);

                    return await handler.HandleAsync(command, currentUser, Context.ConnectionAborted);
                }
                catch (Exception e)
                {
                    return _analyticsService.LogExceptionResponse<TResponse>(this, e, Errors.Errors.Unexpected(e.Message), new Dictionary<string, object>
                    {
                        ["Command"] = command
                    });
                }
            }
        } 
    }
}