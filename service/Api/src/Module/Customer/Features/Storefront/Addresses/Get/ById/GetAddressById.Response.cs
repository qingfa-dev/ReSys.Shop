using Module.Customer.Features.Shared.Addresses.Models;

namespace Module.Customer.Features.Storefront.Addresses.Get.ById;

public static partial class GetAddressById
{
    public sealed record Response() : AddressResponse;
}