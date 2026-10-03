namespace Module.Customer.Features.Storefront.Profiles.Update;

public static partial class UpdateProfile
{
    public sealed record Parameters
    {
        public Guid UserId { get; init; }
        public Request Request { get; init; } = default!;
        public bool IsAdminBypass { get; init; }
    }
}
