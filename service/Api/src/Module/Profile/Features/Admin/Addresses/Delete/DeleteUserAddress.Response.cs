namespace Module.Profile.Features.Admin.Addresses.Delete;

public static partial class DeleteUserAddress
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        public string? Label { get; init; }
    }
}
