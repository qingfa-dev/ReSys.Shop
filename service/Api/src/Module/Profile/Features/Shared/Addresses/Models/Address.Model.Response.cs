namespace Module.Profile.Features.Shared.Addresses.Models;

public record AddressResponse : AddressParameters
{
    public Guid Id { get; init; }
}
