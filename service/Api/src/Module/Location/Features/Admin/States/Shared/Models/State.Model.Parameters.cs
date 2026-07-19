namespace Module.Location.Features.Admin.States.Shared.Models;

public abstract record StateParameters : IActivatableParameters
{
    public string Name { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;
    public Guid CountryId { get; init; }
    public bool IsActive { get; init; } = true;
}