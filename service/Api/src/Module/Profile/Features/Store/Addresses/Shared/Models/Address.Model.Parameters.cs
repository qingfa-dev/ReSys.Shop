using Module.Profile.Domain.Addresses;

namespace Module.Profile.Features.Store.Addresses.Shared.Models;

public abstract class AddressParameters
{
    public AddressType AddressType { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string Address1 { get; init; } = string.Empty;
    public string? Address2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? ZipCode { get; init; }
    public string? Phone { get; init; }
    public string? Label { get; init; }
    public bool IsDefault { get; init; }
    public string CountryName { get; init; } = string.Empty;
    public string? StateProvince { get; init; }
    public string? CountryCode { get; init; }
    public string? StateCode { get; init; }
}