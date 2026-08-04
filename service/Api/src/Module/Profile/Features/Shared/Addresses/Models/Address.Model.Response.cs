namespace Module.Profile.Features.Shared.Addresses.Models;

public record AddressResponse : AddressParameters, IResponse
{
    public Guid Id { get; init; }
}
