using Module.Customer.Features.Shared.Addresses.Models;

namespace Module.Customer.Features.Storefront.Addresses.Update;

public static partial class UpdateAddress
{
    public sealed record Request() : AddressRequest;
}