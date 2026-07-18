namespace Module.Profile.Features.Admin.Addresses.Shared.Models;

public record AddressResponse : AddressParameters
{
    public Guid Id { get; init; }
}
