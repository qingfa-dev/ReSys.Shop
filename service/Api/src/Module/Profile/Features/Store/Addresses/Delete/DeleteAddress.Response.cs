namespace Module.Profile.Features.Store.Addresses.Delete;

public static partial class DeleteAddress
{
    // ============ RESPONSE ============
    // EXCEPTION: minimal delete confirmation — address ID and label only
    public sealed record Response(Guid Id, string Label);
}