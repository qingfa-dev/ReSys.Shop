namespace Module.Payment.Features.Storefront.Payment.Methods;

public static partial class ListPaymentMethods
{
    public record Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
