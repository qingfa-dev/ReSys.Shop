using Module.Profile.Features.Shared.Addresses.Models;

namespace Module.Profile.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public sealed record Response() : AddressResponse;
}
