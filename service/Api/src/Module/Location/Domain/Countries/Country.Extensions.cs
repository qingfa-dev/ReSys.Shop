
using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Location.Domain.Countries;

public static class CountryExtensions
{
    // Contract: pre=name!=null && isoCode!=null && iso3Code!=null, post=entity.Id!=null
    public static Result<Country> Create(
        string name,
        string isoCode,
        string iso3Code,
        string isoName,
        string? callingCode = null,
        bool statesRequired = false,
        bool zipcodeRequired = false,
        bool isActive = true,
        Guid? id = null)
    {
        // Validate: Country name and ISO codes are required
        if (string.IsNullOrWhiteSpace(value: name))
            return CountryResult.Failure.NameRequired;
        if (string.IsNullOrWhiteSpace(value: isoCode))
            return CountryResult.Failure.IsoCodeRequired;

        var country = new Country
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            IsoCode = isoCode,
            Iso3Code = iso3Code,
            IsoName = isoName,
            CallingCode = callingCode,
            StatesRequired = statesRequired,
            ZipcodeRequired = zipcodeRequired,
            IsActive = isActive,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        return country;
    }

    public static Result Update(this Country country,
        string? name = null,
        string? isoName = null,
        string? callingCode = null,
        bool? statesRequired = null,
        bool? zipcodeRequired = null)
    {
        country.Name = name ?? country.Name;
        country.IsoName = isoName ?? country.IsoName;
        country.CallingCode = callingCode ?? country.CallingCode;
        country.StatesRequired = statesRequired ?? country.StatesRequired;
        country.ZipcodeRequired = zipcodeRequired ?? country.ZipcodeRequired;
        AuditableBehavior.Touch(entity: country);
        return Result.Ok();
    }

    // Enforce: Cannot deactivate country with active states
    public static Result Activate(this Country country)
    {
        if (country.IsActive) return Result.Ok();
        country.IsActive = true;
        AuditableBehavior.Touch(entity: country);
        return Result.Ok();
    }

    public static Result Deactivate(this Country country)
    {
        if (!country.IsActive) return Result.Ok();
        // Enforce: Cannot deactivate country with active states
        if (country.States?.Any(predicate: s => s.IsActive) == true)
            return CountryResult.Failure.HasActiveStates;
        country.IsActive = false;
        AuditableBehavior.Touch(entity: country);
        return Result.Ok();
    }

    // Compute: Country has states defined
    public static bool HasStates(this Country country) => country.States?.Count > 0;
}
