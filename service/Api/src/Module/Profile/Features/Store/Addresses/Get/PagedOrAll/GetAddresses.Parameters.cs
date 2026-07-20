namespace Module.Profile.Features.Store.Addresses.Get.PagedOrAll;

public static partial class GetAddresses
{
    public record Parameters : QueryingParameters
    {
        public Guid? UserId { get; init; }
    }
}
