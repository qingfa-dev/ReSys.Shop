using Module.Profile.Features.Shared.Addresses.Models;

namespace Module.Profile.Features.Storefront.Addresses.Get.PagedOrAll;

public static partial class GetAddresses
{
    public sealed record Response() : AddressResponse;
}