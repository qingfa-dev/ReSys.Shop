using GatewayConstants = Module.Billing.Services.Provider.GatewayConstants;

namespace Module.Billing.Services.Models;

public sealed record GatewayOptions
{
    public static string Currency => GatewayConstants.Currency.Usd;

    public required string Email { get; init; }
    public required string Customer { get; init; }
    public string? CustomerId { get; init; }
    public string? Ip { get; init; }
    public required string OrderId { get; init; }
    public required string PaymentId { get; init; }
    public required string IdempotencyKey { get; init; }
    public string? StatementDescriptorSuffix { get; init; }
    public decimal Shipping { get; init; }
    public decimal Tax { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public Dictionary<string, object?>? BillingAddress { get; init; }
    public Dictionary<string, object?>? ShippingAddress { get; init; }
    public Dictionary<string, object?>? ProviderSpecific { get; init; }
}