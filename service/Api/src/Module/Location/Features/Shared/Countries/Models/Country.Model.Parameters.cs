namespace Module.Location.Features.Shared.Countries.Models;

public abstract record class CountryParameters : IActivatableParameters
{
    public string Name { get; init; } = string.Empty;
    public string IsoCode { get; init; } = string.Empty;
    public string? CallingCode { get; init; }
    public bool StatesRequired { get; init; }
    public bool IsActive { get; init; } = true;
}