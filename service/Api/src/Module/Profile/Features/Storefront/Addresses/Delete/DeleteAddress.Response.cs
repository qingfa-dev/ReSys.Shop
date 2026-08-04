namespace Module.Profile.Features.Storefront.Addresses.Delete;

public static partial class DeleteAddress
{
    // ============ RESPONSE ============
    // EXCEPTION: minimal delete confirmation — address ID and label only
    public sealed record Response
    {
        public Guid Id { get; init; }
        public string Label { get; init; } = default!;
    }
}