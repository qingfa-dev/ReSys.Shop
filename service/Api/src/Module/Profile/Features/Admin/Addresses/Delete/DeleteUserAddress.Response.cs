namespace Module.Profile.Features.Admin.Addresses.Delete;

public static partial class DeleteUserAddress
{
    public sealed record Response(Guid Id, string? Label);
}
