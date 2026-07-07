using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Module.Ordering.Domain.Orders;
using Shared.Operational.Persistence.Data;

namespace Module.Ordering.Backgrounds;

public sealed class CartExpiryJob
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
        var expiredCarts = await _dbContext.Set<Order>()
            .Where(o => o.Status == OrderStatus.Draft && o.ModifiedAtUtc < cutoff && !o.IsDeleted)
            .ToListAsync(ct);

        _logger.LogInformation("Cart-expiry job found {Count} drafts to expire", expiredCarts.Count);

        foreach (var cart in expiredCarts)
        {
            cart.Status = OrderStatus.Expired;
            cart.IsDeleted = true;
            cart.DeletedAtUtc = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
