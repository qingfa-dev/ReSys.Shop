using Module.Location.Domain.States;
using Module.Location.Features.Shared.States.Models;

using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Location.Features.Shared.States.Mappings;

public static partial class StateMapping
{
    public static State MapToDomain<T>(this T request) where T : StateRequest
    {
        var state = new State
        {
            Name = request.Name,
            Abbreviation = request.Abbreviation,
            CountryId = request.CountryId,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        AuditableBehavior.Create(entity: state);
        return state;
    }

    public static void MapToDomain<T>(this T request, State state) where T : StateRequest
    {
        state.Name = request.Name;
        state.Abbreviation = request.Abbreviation;
        state.CountryId = request.CountryId;
        state.IsActive = request.IsActive;

        AuditableBehavior.Touch(entity: state);
    }
}