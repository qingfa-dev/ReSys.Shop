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
            .ToListAsync(ct);

        Loggers.Found(_logger, expired.Count, cutoff);

        foreach (var cart in expired)
        {
            cart.Status = OrderStatus.Expired;
            cart.IsDeleted = true;
            cart.DeletedAtUtc = DateTimeOffset.UtcNow;
        }

        Loggers.Completed(_logger, expired.Count);
        await _dbContext.SaveChangesAsync(ct);
    }
}
