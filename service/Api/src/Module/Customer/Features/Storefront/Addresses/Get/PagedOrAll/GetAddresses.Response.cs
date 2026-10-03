using Module.Customer.Features.Shared.Addresses.Models;

namespace Module.Customer.Features.Storefront.Addresses.Get.PagedOrAll;

public static partial class GetAddresses
{
    public sealed record Response() : AddressResponse;
}