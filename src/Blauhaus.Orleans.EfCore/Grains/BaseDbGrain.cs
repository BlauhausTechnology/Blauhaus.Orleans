using System;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Orleans.Grains;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Orleans.EfCore.Grains
{
    public abstract class BaseDbGrain<TDbContext> : BaseIdGrain where TDbContext : DbContext
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
    }
}