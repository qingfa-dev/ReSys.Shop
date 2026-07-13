using Microsoft.EntityFrameworkCore;

namespace Module.Ordering.Domain.Orders;

public static class OrderNumber
{
    private const int MaxAttempts = 8;

    public static Result<string> Generate(IApplicationDbContext dbContext)
    {
        for (var attempts = 1; attempts <= MaxAttempts; attempts++)
        {
            var candidate = $"R{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            var exists = dbContext.Set<Order>().Any(o => o.Number == candidate);
            if (!exists) return candidate;
        }
        return OrderResult.Errors.OrderNumberGenerationFailed;
    }
}
