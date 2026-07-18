namespace Module.Profile.Features.Admin.Addresses.Delete;

public static partial class DeleteAddress
{
    public sealed record Response(Guid Id, string? Label);
}
