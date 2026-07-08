namespace Module.Payment.Domain.Payments;
/// <summary>Represents a Gateway Options.</summary>

// Invariant: Currency is ISO 4217 3-letter code; OrderId format is "{order_number}-{payment_number}"
// Define: Gateway options builder ported from Spree::Payment::GatewayOptions — constructs the options hash sent to payment gateways
public sealed class GatewayOptions
{
    private readonly PaymentRecord _payment;

    public GatewayOptions(PaymentRecord payment)
    {
        _payment = payment;
    }

    // Compute: ISO 4217 currency code — mirrors Ruby delegate :currency, to: :payment
    public static string Currency => "USD";

    // Compute: Customer email — mirrors Ruby delegate :email, to: :order
    public required string Email { get; init; }

    // Compute: Order number as statement descriptor suffix — mirrors Ruby statement_descriptor_suffix
    public required string StatementDescriptorSuffix { get; init; }

    // Compute: Customer identifier (email) — mirrors Ruby customer
    public required string Customer { get; init; }

    // Compute: Customer user identifier — mirrors Ruby customer_id
    public required string? CustomerId { get; init; }

    // Compute: Last IP address from order — mirrors Ruby ip
    public required string? Ip { get; init; }

    // Compute: Unique order identifier "{order_number}-{payment_number}" — mirrors Ruby order_id
    public required string OrderId { get; init; }

    // Compute: Payment number — mirrors Ruby payment_id
    public required string PaymentId { get; init; }

    // Compute: Idempotency key "spree-{payment_number}" — mirrors Ruby idempotency_key
    public required string IdempotencyKey { get; init; }

    // Compute: Shipping total — mirrors Ruby shipping
    public decimal Shipping { get; init; }

    // Compute: Tax total — mirrors Ruby tax
    public decimal Tax { get; init; }

    // Compute: Subtotal — mirrors Ruby subtotal
    public decimal Subtotal { get; init; }

    // Compute: Discount total — mirrors Ruby discount
    public decimal Discount { get; init; }

    // Compute: Billing address hash — mirrors Ruby billing_address
    public Dictionary<string, object?>? BillingAddress { get; init; }

    // Compute: Shipping address hash — mirrors Ruby shipping_address
    public Dictionary<string, object?>? ShippingAddress { get; init; }

    // Transform: Convert gateway options to dictionary for gateway API — mirrors Ruby to_hash
    public Dictionary<string, object?> ToHash()
    {
        return new Dictionary<string, object?>
        {
            ["email"] = Email,
            ["customer"] = Customer,
            ["customer_id"] = CustomerId,
            ["ip"] = Ip,
            ["order_id"] = OrderId,
            ["payment_id"] = PaymentId,
            ["idempotency_key"] = IdempotencyKey,
            ["shipping"] = Shipping,
            ["tax"] = Tax,
            ["subtotal"] = Subtotal,
            ["discount"] = Discount,
            ["currency"] = Currency,
            ["billing_address"] = BillingAddress,
            ["shipping_address"] = ShippingAddress,
            ["statement_descriptor_suffix"] = StatementDescriptorSuffix
        };
    }
}