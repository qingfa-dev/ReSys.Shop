namespace Module.Profile.Features.Admin.Addresses.Shared.Models;

public record AddressResponse : AddressParameters, IResponse
{
    public Guid Id { get; init; }
}
