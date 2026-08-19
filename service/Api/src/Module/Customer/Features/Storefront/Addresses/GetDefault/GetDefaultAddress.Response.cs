using Module.Customer.Features.Shared.Addresses.Models;

namespace Module.Customer.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public sealed record Response() : AddressResponse;
}
