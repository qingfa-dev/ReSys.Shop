namespace Module.Customer.Features.Shared.Addresses.Models;

public record AddressResponse : AddressParameters
{
    public Guid Id { get; init; }
}
