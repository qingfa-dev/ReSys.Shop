using Microsoft.EntityFrameworkCore;

namespace Module.Ordering.Domain.Orders;

// Generate: Unique order numbers with prefix format R{yyyMMdd}-{random} — up to MaxAttempts retries on collision
public static class OrderNumber
{
    private const int MaxAttempts = 8;

    // Generate: Candidate uses date prefix for human readability and 8-hex-char suffix for uniqueness
    public static async Task<Result<string>> GenerateAsync(IApplicationDbContext dbContext, CancellationToken ct = default)
    {
        for (var attempts = 1; attempts <= MaxAttempts; attempts++)
        {
            var candidate = $"R{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            var exists = await dbContext.Set<Order>().AnyAsync(o => o.Number == candidate, ct);
            if (!exists) return candidate;
        }
        return OrderResult.Errors.OrderNumberGenerationFailed;
    }
}