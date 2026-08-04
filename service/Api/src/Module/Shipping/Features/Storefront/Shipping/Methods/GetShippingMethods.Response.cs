namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    // EXCEPTION: DTO mapped from domain ShippingMethod entities — no single shipping method entity
    public sealed record Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? AdminName { get; init; }
        public string? Code { get; init; }
        public string CalculatorType { get; init; } = default!;
        public int Position { get; init; }
    }
}
