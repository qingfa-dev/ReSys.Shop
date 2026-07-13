using Hangfire;

using Microsoft.Extensions.Hosting;

namespace Module.Ordering.Backgrounds;

// Trigger: Hangfire recurring job scheduler for CartExpiryJob — registered as IHostedService for automatic startup
public sealed class CartExpiryJobScheduler : IHostedService
{
    private readonly ILogger<CartExpiryJobScheduler> _logger;

    public CartExpiryJobScheduler(ILogger<CartExpiryJobScheduler> logger)
    {
        _logger = logger;
    }

    // Trigger: Register recurring Hangfire job on application start
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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}