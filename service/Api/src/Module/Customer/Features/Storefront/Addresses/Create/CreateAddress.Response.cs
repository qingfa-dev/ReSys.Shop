using Module.Customer.Features.Shared.Addresses.Models;

namespace Module.Customer.Features.Storefront.Addresses.Create;

public static partial class CreateAddress
{
    public sealed record Response() : AddressResponse;
}