using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Errors;
using Blauhaus.Orleans.Grains;
using Blauhaus.Responses;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseDbGrain<TDbContext> : BaseGrain 
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