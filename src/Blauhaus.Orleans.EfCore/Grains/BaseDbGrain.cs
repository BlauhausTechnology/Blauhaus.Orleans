using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Errors;
using Blauhaus.Auth.Abstractions.Extensions;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Errors;
using Blauhaus.Errors.Extensions;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.Grains;
using Blauhaus.Responses;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using Orleans;
using static Blauhaus.Errors.Errors;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseDbGrain<TDbContext, TGrainResolver> : BaseResolverGrain<TGrainResolver>
        where TDbContext : DbContext where TGrainResolver : IGrainResolver
    {

        protected readonly Func<TDbContext> GetDbContext;
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;

        protected DateTime Now => TimeService.CurrentUtcTime;

        protected BaseDbGrain(
            Func<TDbContext> dbContextFactory,
            IAnalyticsService analyticsService,
            ITimeService timeService,
            TGrainResolver grainResolver) : base(grainResolver)
        {
            GetDbContext = dbContextFactory;
            AnalyticsService = analyticsService;
            TimeService = timeService;
        }
        
        protected async Task TryExecuteAsync(Func<Task> func, string? trace = null)
        {
            IDisposable? traceHandle = null;
            if (trace != null)
            {
                traceHandle = AnalyticsService.StartTrace(this, trace);
            }
            try
            {
                await func.Invoke();
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
            }
            finally
            {
                traceHandle?.Dispose();
            }
        }

        protected async Task<Response> TryUpdateDbAsync(Func<TDbContext, DateTime, Task<Response>> func, string? trace = null)
        {
            IDisposable? traceHandle = null;
            if (trace != null)
            {
                traceHandle = AnalyticsService.StartTrace(this, trace);
            }

            try
            {
                using var db = GetDbContext();
                var response = await func.Invoke(db, TimeService.CurrentUtcTime);
                if (response.IsSuccess && db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }

                return response;
            }
            catch (Exception e)
            {
                if (e.IsErrorException())
                {
                    return AnalyticsService.TraceErrorResponse(this, e.ToError());
                }
                else
                {
                    AnalyticsService.LogException(this, e);
                    return Response.Failure(Unexpected("failed to complete database operation"));
                }
            }
            finally
            {
                traceHandle?.Dispose();
            }
        }
        
        protected async Task TryUpdateDbAsync(Func<TDbContext, DateTime, Task> func, string? trace = null)
        {
            IDisposable? traceHandle = null;
            if (trace != null)
            {
                traceHandle = AnalyticsService.StartTrace(this, trace);
            }

            try
            {
                using var db = GetDbContext();
                await func.Invoke(db, TimeService.CurrentUtcTime);
                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                AnalyticsService.LogException(this, e);
            }
            finally
            {
                traceHandle?.Dispose();
            }
        }

        protected  Task<Response> TryExecuteDbCommandAsync<TCommand>(TCommand command, Func<TDbContext, DateTime, Task<Response>> func)
        {
            return TryExecuteCommandAsync(command, async () =>
            {
                using (var db = GetDbContext())
                {
                    var response = await func.Invoke(db, Now);
                    if (response.IsSuccess && db.ChangeTracker.HasChanges())
                    {
                        await db.SaveChangesAsync();
                    }
                    return response;
                }
            }); 
        }

        protected async Task<Response> TryExecuteCommandAsync<TCommand>(TCommand command, Func<Task<Response>> func)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand).Name} executed by {this.GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary()))
            {
                try
                {
                    using (var db = GetDbContext())
                    {
                        var response = await func.Invoke();
                        if (response.IsSuccess && db.ChangeTracker.HasChanges())
                        {
                            await db.SaveChangesAsync();
                        }
                        return response;
                    }
                }
                catch (Exception e)
                {
                    if (e.IsErrorException())
                    {
                        return AnalyticsService.TraceErrorResponse(this, e.ToError());
                    }
                    AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                    return Response.Failure(Unexpected($"{typeof(TCommand).Name} failed to complete"));
                }
            }
        }


        protected  Task<Response<TResponse>> TryExecuteDbCommandAsync<TResponse, TCommand>(TCommand command, Func<TDbContext, DateTime, Task<Response<TResponse>>> func)
        {
            return TryExecuteCommandAsync(command, async () =>
            {
                using (var db = GetDbContext())
                {
                    var response = await func.Invoke(db, Now);
                    if (response.IsSuccess)
                    {
                        await db.SaveChangesAsync();
                    }
                    return response;
                }
            }); 
        }

        protected async Task<Response<TResponse>> TryExecuteCommandAsync<TResponse, TCommand>(TCommand command, Func<Task<Response<TResponse>>> func)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand).Name} executed by {this.GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary()))
            {
                try
                {
                    using (var db = GetDbContext())
                    {
                        var response = await func.Invoke();
                        if (response.IsSuccess)
                        {
                            await db.SaveChangesAsync();
                        }
                        return response;
                    }
                }
                catch (Exception e)
                {
                    if (e.IsErrorException())
                    {
                        return AnalyticsService.TraceErrorResponse<TResponse>(this, e.ToError());
                    }
                    AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                    return Response.Failure<TResponse>(Unexpected($"{typeof(TCommand).Name} failed to return {typeof(TResponse).Name}"));
                }
            }
        }

        protected  Task<Response<TResponse>> TryExecuteAdminDbCommandAsync<TCommand, TResponse>(TCommand command, IAuthenticatedUser user, Func<TDbContext, DateTime, Task<Response<TResponse>>> func)
        {
            return TryExecuteCommandAsync(command, () =>
            {
                if (!user.IsAdminUser()) 
                    return TraceErrorTask<TResponse>(AuthError.NotAuthorized);

                return TryExecuteDbCommandAsync(command, func); 
            }); 
        }
        
        protected  Task<Response> TryExecuteAdminDbCommandAsync<TCommand>(TCommand command, IAuthenticatedUser user, Func<TDbContext, DateTime, Task<Response>> func)
        {
            return TryExecuteCommandAsync(command, () =>
            {
                if (!user.IsAdminUser()) 
                    return TraceErrorTask(AuthError.NotAuthorized);

                return TryExecuteDbCommandAsync(command, func); 
            }); 
        }



        protected Response TraceError(Error error)
        {
            return AnalyticsService.TraceErrorResponse(this, error);
        }    
        
        protected Response<T> TraceError<T>(Error error)
        {
            return AnalyticsService.TraceErrorResponse<T>(this, error);
        }    

        protected Task<Response<T>> TraceErrorTask<T>(Error error)
        {
            return Task.FromResult(TraceError<T>(error));
        }    
        protected Task<Response> TraceErrorTask(Error error)
        {
            return Task.FromResult(TraceError(error));
        }    

        protected Response TraceError(Response reponse)
        {
            return AnalyticsService.TraceErrorResponse(this, reponse.Error);
        }        
        
        protected Response TraceError<T>(Response<T> reponse)
        {
            return AnalyticsService.TraceErrorResponse(this, reponse.Error);
        }        
        
        protected Response<T> TraceResponseError<T>(Response<T> reponse)
        {
            return AnalyticsService.TraceErrorResponse<T>(this, reponse.Error);
        }        
        
        protected Response TraceError(Error error, Dictionary<string, object> properties)
        {
            return AnalyticsService.TraceErrorResponse(this, error);
        }
        
        protected Response<T> TraceErrorWarning<T>(Error error, Dictionary<string, object>? properties = null)
        {
            AnalyticsService.TraceWarning(this, error.Description, properties ?? new Dictionary<string, object>());
            return Response.Failure<T>(error);
        }
        
        protected Response TraceErrorWarning(Error error, Dictionary<string, object>? properties = null)
        {
            AnalyticsService.TraceWarning(this, error.Description, properties ?? new Dictionary<string, object>());
            return Response.Failure(error);
        }
    }

    public abstract class BaseDbGrain<TDbContext> : Grain 
        where TDbContext : DbContext
    {
        protected readonly Func<TDbContext> GetDbContext;
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;

        protected DateTime Now => TimeService.CurrentUtcTime;

        protected BaseDbGrain(
            Func<TDbContext> dbContextFactory,
            IAnalyticsService analyticsService,
            ITimeService timeService)
        {
            GetDbContext = dbContextFactory;
            AnalyticsService = analyticsService;
            TimeService = timeService;
        }
        
        protected Response TraceError(Error error)
        {
            return AnalyticsService.TraceErrorResponse(this, error);
        }    
        
        protected Response<T> TraceError<T>(Error error)
        {
            return AnalyticsService.TraceErrorResponse<T>(this, error);
        }    

        protected Task<Response<T>> TraceErrorTask<T>(Error error)
        {
            return Task.FromResult(TraceError<T>(error));
        }    
        protected Task<Response> TraceErrorTask(Error error)
        {
            return Task.FromResult(TraceError(error));
        }    

        protected Response TraceError(Response reponse)
        {
            return AnalyticsService.TraceErrorResponse(this, reponse.Error);
        }        
        
        protected Response TraceError<T>(Response<T> reponse)
        {
            return AnalyticsService.TraceErrorResponse(this, reponse.Error);
        }        
        
        protected Response<T> TraceResponseError<T>(Response<T> reponse)
        {
            return AnalyticsService.TraceErrorResponse<T>(this, reponse.Error);
        }        
        
        protected Response TraceError(Error error, Dictionary<string, object> properties)
        {
            return AnalyticsService.TraceErrorResponse(this, error);
        }
        
        protected Response<T> TraceErrorWarning<T>(Error error, Dictionary<string, object>? properties = null)
        {
            AnalyticsService.TraceWarning(this, error.Description, properties ?? new Dictionary<string, object>());
            return Response.Failure<T>(error);
        }
        
        protected Response TraceErrorWarning(Error error, Dictionary<string, object>? properties = null)
        {
            AnalyticsService.TraceWarning(this, error.Description, properties ?? new Dictionary<string, object>());
            return Response.Failure(error);
        }
    }
}