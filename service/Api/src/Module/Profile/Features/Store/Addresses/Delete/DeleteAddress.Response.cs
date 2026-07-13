namespace Module.Profile.Features.Store.Addresses.Delete;

public static partial class DeleteAddress
{
    // ============ RESPONSE ============
    public sealed record Response(Guid Id, string Label);
}