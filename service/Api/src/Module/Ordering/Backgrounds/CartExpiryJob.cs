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
        var expired = await _dbContext.Set<Order>()
            .Where(o => o.Status == OrderStatus.Draft && o.ModifiedAtUtc < cutoff && !o.IsDeleted)
            .AsNoTracking()
            .Select(o => new { o.Id, o.Status, o.ModifiedAtUtc, o.IsDeleted })
            .ToListAsync(ct);

        _logger.LogInformation("Cart-expiry job found {Count} drafts to expire", expired.Count);

        foreach (var cart in expired)
        {
            await _dbContext.Set<Order>()
                .Where(o => o.Id == cart.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.Status, OrderStatus.Expired)
                    .SetProperty(o => o.IsDeleted, true)
                    .SetProperty(o => o.DeletedAtUtc, DateTimeOffset.UtcNow), ct);
        }

        _logger.LogInformation("Cart-expiry job completed: {Count} drafts expired", expired.Count);
    }
}
