using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Location.Domain.States;

public static class StateExtensions
{
    // Contract: pre=name!=null && abbreviation!=null && countryId!=Guid.Empty, post=entity.Id!=null
    public static Result<State> Create(
        string name,
        string abbreviation,
        Guid countryId,
        bool isActive = true,
        Guid? id = null)
    {
        // Validate: State name, abbreviation, and country are required
        if (string.IsNullOrWhiteSpace(value: name))
            return StateResult.Errors.NameRequired;
        if (string.IsNullOrWhiteSpace(value: abbreviation))
            return StateResult.Errors.AbbreviationRequired;
        if (countryId == Guid.Empty)
            return StateResult.Errors.CountryRequired;

        var state = new State
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Abbreviation = abbreviation,
            CountryId = countryId,
            IsActive = isActive,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        return state;
    }

    public static Result Update(this State state,
        string? name = null,
        string? abbreviation = null)
    {
        state.Name = name ?? state.Name;
        state.Abbreviation = abbreviation ?? state.Abbreviation;
        AuditableBehavior.Touch(entity: state);
        return Result.Ok();
    }

    // Enforce: Activate an inactive state
    public static Result Activate(this State state)
    {
        if (state.IsActive) return Result.Ok();
        state.IsActive = true;
        AuditableBehavior.Touch(entity: state);
        return Result.Ok();
    }

    // Enforce: Deactivate an active state
    public static Result Deactivate(this State state)
    {
        if (!state.IsActive) return Result.Ok();
        state.IsActive = false;
        AuditableBehavior.Touch(entity: state);
        return Result.Ok();
    }
}