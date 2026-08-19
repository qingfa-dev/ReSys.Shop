using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Backgrounds;

/// <summary>Background job that expires draft carts past a configurable inactivity cutoff.</summary>
// Contract: pre=dbContext!=null && logger!=null, post=expired carts have Status==Expired && IsDeleted==true
public sealed partial class CartExpiryJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CartExpiryJob> _logger;
    private readonly int _afterDays;

    internal const int BatchSize = 500;

    public CartExpiryJob(IApplicationDbContext dbContext, ILogger<CartExpiryJob> logger, int afterDays = 7)
    {
        _dbContext = dbContext;
        _logger = logger;
        _afterDays = afterDays;
    }

    /// <summary>Executes the expiry sweep — transitions draft carts past the cutoff to Expired with soft-delete, in batches.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunAsync(CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-_afterDays);
        var totalExpired = 0;

        List<Order> expired;
        do
        {
            expired = await _dbContext.Set<Order>()
                .Where(o => o.Status == OrderStatus.Draft
                    && (o.ModifiedAtUtc == null || o.ModifiedAtUtc < cutoff)
                    && !o.IsDeleted)
                .Take(BatchSize)
                .ToListAsync(ct);

            foreach (var cart in expired)
            {
                cart.Status = OrderStatus.Expired;
                cart.Delete(OrderConstant.Defaults.CreatedBy);
            }

            totalExpired += expired.Count;
            await _dbContext.SaveChangesAsync(ct);

            Loggers.Found(_logger, expired.Count, cutoff);
        } while (expired.Count == BatchSize);

        Loggers.Completed(_logger, totalExpired);
    }
}
