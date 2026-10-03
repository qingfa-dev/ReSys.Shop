using Hangfire;

using Microsoft.Extensions.Hosting;

namespace Module.Ordering.Backgrounds;

/// <summary>Hangfire recurring job scheduler for CartExpiryJob — registered as IHostedService for automatic startup.</summary>
public sealed class CartExpiryJobScheduler : IHostedService
{
    private readonly ILogger<CartExpiryJobScheduler> _logger;

    public CartExpiryJobScheduler(ILogger<CartExpiryJobScheduler> logger)
    {
        _logger = logger;
    }

    /// <summary>Registers the Hangfire recurring job for cart expiry on application start.</summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Schedule: Cart expiry recurring job runs on the configured cron interval
        RecurringJob.AddOrUpdate<CartExpiryJob>(
            CartExpiryJobConstants.Scheduler.JobId,
            job => job.RunAsync(CancellationToken.None),
            CartExpiryJobConstants.Scheduler.CronExpression);

        // Log: Confirm scheduler registration for operational visibility
        CartExpiryJob.Loggers.SchedulerRegistered(
            _logger,
            CartExpiryJobConstants.Scheduler.JobId,
            CartExpiryJobConstants.Scheduler.CronExpression);

        return Task.CompletedTask;
    }

    /// <summary>No-op stop — Hangfire manages job lifecycle independently.</summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}