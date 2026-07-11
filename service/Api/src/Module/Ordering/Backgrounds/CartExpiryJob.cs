using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Backgrounds;

public sealed partial class CartExpiryJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CartExpiryJob> _logger;
    private readonly int _afterDays;

    public CartExpiryJob(IApplicationDbContext dbContext, ILogger<CartExpiryJob> logger, int afterDays = 7)
    {
        _dbContext = dbContext;
        _logger = logger;
        _afterDays = afterDays;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-_afterDays);
        var expired = await _dbContext.Set<Order>()
            .Where(o => o.Status == OrderStatus.Draft && o.ModifiedAtUtc < cutoff && !o.IsDeleted)
            .AsNoTracking()
            .Select(o => new { o.Id, o.Status, o.ModifiedAtUtc, o.IsDeleted })
            .ToListAsync(ct);

        Loggers.Found(_logger, expired.Count, cutoff);

        foreach (var cart in expired)
        {
            await _dbContext.Set<Order>()
                .Where(o => o.Id == cart.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.Status, OrderStatus.Expired)
                    .SetProperty(o => o.IsDeleted, true)
                    .SetProperty(o => o.DeletedAtUtc, DateTimeOffset.UtcNow), ct);
        }

        Loggers.Completed(_logger, expired.Count);
    }
}
