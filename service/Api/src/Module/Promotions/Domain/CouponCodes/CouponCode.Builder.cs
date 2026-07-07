namespace Module.Promotions.Domain.CouponCodes;

// Generate: Batch coupon code generation for multi-code promotions
public static class CouponCodeBuilder
{
    #region Build
    /// <summary>Generates a batch of unique coupon codes for a promotion.</summary>
    /// <param name="promotionId">The promotion identifier.</param>
    /// <param name="count">Number of codes to generate.</param>
    /// <param name="prefix">Optional prefix for generated codes.</param>
    /// <param name="codeLength">Length of the random portion (default 8 hex chars).</param>
    /// <returns>A list of generated CouponCode entities.</returns>
    // Contract: pre=count > 0 && count <= MAX_BATCH_SIZE, post=result.Count == count, throws=none
    public static List<CouponCode> Build(Guid promotionId, int count, string? prefix = null, int codeLength = 8)
    {
        // Guard: Prevent excessive batch generation beyond configured limit
        if (count <= 0) return [];
        if (count > MaxBatchSize) count = MaxBatchSize;

        var codes = new List<CouponCode>(count);
        var existing = new HashSet<string>();

        for (var i = 0; i < count; i++)
        {
            // Generate: Unique hex code with dedup check against previously generated codes in this batch
            var code = GenerateUniqueCode(prefix, codeLength, existing);
            existing.Add(code);

            var couponCode = new CouponCode
            {
                Id = Guid.NewGuid(),
                Code = code,
                PromotionId = promotionId,
                State = CouponCodeState.Active,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                CreatedBy = "System"
            };
            codes.Add(couponCode);
        }

        return codes;
    }
    #endregion Build

    #region Constants
    /// <summary>Maximum number of codes that can be generated in a single batch.</summary>
    public const int MaxBatchSize = 10_000;
    #endregion Constants

    #region GenerateUniqueCode
    // Generate: Random hex string with optional prefix, deduplicated against existing set
    private static string GenerateUniqueCode(string? prefix, int length, HashSet<string> existing)
    {
        var prefixStr = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}-";

        string code;
        do
        {
            code = $"{prefixStr}{Guid.NewGuid().ToString("N")[..length]}";
        } while (existing.Contains(code));

        return code;
    }
    #endregion GenerateUniqueCode
}