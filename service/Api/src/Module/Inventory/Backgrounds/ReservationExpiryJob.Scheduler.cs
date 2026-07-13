using Hangfire;

using Microsoft.Extensions.Hosting;

namespace Module.Inventory.Backgrounds;

public sealed class ReservationExpiryJobScheduler : IHostedService
{
    private readonly ILogger<ReservationExpiryJobScheduler> _logger;

    public ReservationExpiryJobScheduler(ILogger<ReservationExpiryJobScheduler> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RecurringJob.AddOrUpdate<ReservationExpiryJob>(
            ReservationExpiryJobConstants.Scheduler.JobId,
            job => job.RunAsync(CancellationToken.None),
            ReservationExpiryJobConstants.Scheduler.CronExpression);

        ReservationExpiryJobLoggers.SchedulerRegistered(
            _logger,
            ReservationExpiryJobConstants.Scheduler.JobId,
            ReservationExpiryJobConstants.Scheduler.CronExpression);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}