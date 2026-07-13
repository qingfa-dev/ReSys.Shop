using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Backgrounds;

// @CAT-10 Contract: pre=dbContext!=null && logger!=null, post=expired carts have Status==Expired && IsDeleted==true
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

    // Filter: Identify draft carts past the inactivity cutoff — excludes already-deleted records
    public async Task RunAsync(CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-_afterDays);

        // Filter: Draft carts not modified within the expiry window and not already soft-deleted
        var expired = await _dbContext.Set<Order>()
            .Where(o => o.Status == OrderStatus.Draft
                && (o.ModifiedAtUtc == null || o.ModifiedAtUtc < cutoff)
                && !o.IsDeleted)
            .ToListAsync(ct);

        // Log: Number of expired carts found for monitoring and alerting
        Loggers.Found(_logger, expired.Count, cutoff);

        // Update: Transition each expired cart to Expired status and soft-delete
        foreach (var cart in expired)
        {
            cart.Status = OrderStatus.Expired;
            cart.IsDeleted = true;
            cart.DeletedAtUtc = DateTimeOffset.UtcNow;
        }

        // Log: Completion count for observability in dashboard
        Loggers.Completed(_logger, expired.Count);
        await _dbContext.SaveChangesAsync(ct);
    }
}
