using Hangfire;

using Microsoft.Extensions.Hosting;

namespace Module.Ordering.Backgrounds;

public sealed class CartExpiryJobScheduler : IHostedService
{
    private readonly ILogger<CartExpiryJobScheduler> _logger;

    public CartExpiryJobScheduler(ILogger<CartExpiryJobScheduler> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RecurringJob.AddOrUpdate<CartExpiryJob>(
            CartExpiryJobConstants.Scheduler.JobId,
            job => job.RunAsync(CancellationToken.None),
            CartExpiryJobConstants.Scheduler.CronExpression);

        CartExpiryJob.Loggers.SchedulerRegistered(
            _logger,
            CartExpiryJobConstants.Scheduler.JobId,
            CartExpiryJobConstants.Scheduler.CronExpression);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
