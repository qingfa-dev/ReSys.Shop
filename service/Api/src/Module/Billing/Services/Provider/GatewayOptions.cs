namespace Module.Billing.Services.Provider;

/// <summary>Required and optional parameters passed to gateway payment operations.</summary>
public sealed record GatewayOptions
{
    public string Currency { get; init; } = GatewayConstants.Currency.Usd;

    public required string Email { get; init; }
    public required string Customer { get; init; }
    public string? CustomerId { get; init; }
    public string? Ip { get; init; }
    public required string OrderId { get; init; }
    public required string PaymentId { get; init; }
    public required string IdempotencyKey { get; init; }
    public string? StatementDescriptorSuffix { get; init; }
    public string? SuccessUrl { get; init; }
    public string? CancelUrl { get; init; }
    public decimal Shipping { get; init; }
    public string? ShippingDisplayName { get; init; }
    public IReadOnlyList<GatewayLineItem> LineItems { get; init; } = [];
    public decimal Tax { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public Dictionary<string, object?>? BillingAddress { get; init; }
    public Dictionary<string, object?>? ShippingAddress { get; init; }
    public Dictionary<string, object?>? ProviderSpecific { get; init; }
}

public sealed record GatewayLineItem(string Name, int Quantity, decimal UnitPrice);