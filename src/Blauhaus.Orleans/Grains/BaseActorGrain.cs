using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Errors;
using Blauhaus.Auth.Abstractions.Extensions;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Actors;
using Blauhaus.Domain.Abstractions.Commands;
using Blauhaus.Errors;
using Blauhaus.Errors.Extensions;
using Blauhaus.Orleans.Abstractions.Grains;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Responses;
using Orleans;
using Response = Blauhaus.Responses.Response;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseActorGrain<TGrainResolver, TActor, TModel, TDto> : BaseActorGrain<TGrainResolver, TActor, TModel>, IActorGrain<TModel, TDto>
        where TModel : IHasId<Guid>
        where TActor : IDtoModelActor<TModel, TDto, Guid>
        where TDto : IHasId<Guid>
        where TGrainResolver : IGrainResolver
    {
        protected BaseActorGrain(
            IAnalyticsService analyticsService, 
            TGrainResolver grainResolver, 
            TActor actor) 
                : base(analyticsService, grainResolver, actor)
        {
        }

        public Task<TDto> GetDtoAsync()
        {
            return Actor.GetDtoAsync();
        }
    }

    public abstract class BaseActorGrain<TGrainResolver, TActor, TModel> : BaseResolverGrain<TGrainResolver>, IActorGrain<TModel>
        where TModel : IHasId<Guid>
        where TActor : IModelActor<TModel, Guid>
        where TGrainResolver : IGrainResolver
    {
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly TActor Actor;
        protected Guid Id;

        protected BaseActorGrain(
            IAnalyticsService analyticsService, 
            TGrainResolver grainResolver,
            TActor actor) 
            : base(grainResolver)
        {
            AnalyticsService = analyticsService;
            Actor = actor;
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                await base.OnActivateAsync();

                Id = this.GetPrimaryKey();
                await Actor.InitializeAsync(Id);

                if (Id == Guid.Empty)
                {
                    throw new ArgumentException($"Grain requires a GUID id. \"{this.GetPrimaryKey()}\" is not valid");
                }
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
                throw;
            }
        }

        public Task<TModel> GetModelAsync()
        {
            return Actor.GetModelAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            await Actor.DisposeAsync();
        }
        
        protected async Task<Response> TryExecuteCommandAsync<TCommand>(TCommand command, IAuthenticatedUser user, Func<Task<Response>> func)
        {
            using var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand)} executed by {GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary());

            try
            {
                if (command is IAdminCommand)
                {
                    if (!user.IsAdminUser())
                    {
                        return AnalyticsService.TraceErrorResponse(this, AuthError.NotAuthorized);
                    }
                }
                return await func.Invoke();
            }
            catch (Exception e)
            {
                if (e.IsErrorException())
                {
                    return AnalyticsService.TraceErrorResponse(this, e.ToError());
                }
                AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                return Response.Failure(Error.Unexpected($"{typeof(TCommand)} failed to complete"));
            }
        }
        protected async Task<Response> TryExecuteAsync(Func<Task<Response>> func, string operationName, Dictionary<string, object>? properties = null)
        {
            using var _ = AnalyticsService.StartTrace(this, $"{operationName} executed by {GetType().Name}", LogSeverity.Verbose, properties);

            try
            {
                return await func.Invoke();
            }
            catch (Exception e)
            {
                if (e.IsErrorException())
                {
                    return AnalyticsService.TraceErrorResponse(this, e.ToError());
                }
                AnalyticsService.LogException(this, e, properties);
                return Response.Failure(Error.Unexpected($"{operationName} failed to complete"));
            }
        }
        
        protected async Task<Response<TResponse>> TryExecuteCommandAsync<TResponse, TCommand>(TCommand command, IAuthenticatedUser user, Func<Task<Response<TResponse>>> func)
        {
            using var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand)} executed by {GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary());

            try
            {
                if (command is IAdminCommand)
                {
                    if (!user.IsAdminUser())
                    {
                        return AnalyticsService.TraceErrorResponse<TResponse>(this, AuthError.NotAuthorized);
                    }
                }
                return await func.Invoke();
            }
            catch (Exception e)
            {
                if (e.IsErrorException())
                {
                    return AnalyticsService.TraceErrorResponse<TResponse>(this, e.ToError());
                }
                AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                return Response.Failure<TResponse>(Error.Unexpected($"{typeof(TCommand)} failed to complete"));
            }
        }
        protected async Task<Response<T>> TryExecuteAsync<T>(Func<Task<Response<T>>> func, string operationName, Dictionary<string, object>? properties = null)
        {
            using var _ = AnalyticsService.StartTrace(this, $"{operationName} executed by {GetType().Name}", LogSeverity.Verbose, properties);

            try
            {
                return await func.Invoke();
            }
            catch (Exception e)
            {
                if (e.IsErrorException())
                {
                    return AnalyticsService.TraceErrorResponse<T>(this, e.ToError());
                }
                AnalyticsService.LogException(this, e, properties);
                return Response.Failure<T>(Error.Unexpected($"{operationName} failed to complete"));
            }
        }
    }
}