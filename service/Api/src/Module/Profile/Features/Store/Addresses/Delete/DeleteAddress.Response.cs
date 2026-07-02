namespace Module.Profile.Features.Store.Addresses.Delete;

public static partial class DeleteAddress
{
    // ============ RESPONSE ============
    public record Response(Guid Id, string Label);
}
