using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Models;

namespace Module.Location.Features.Admin.States.Shared.Mappings;

public static partial class StateMapping
{
    public static T MapToDetail<T>(this State state) where T : StateDetailResponse, new()
    {
        return new T
        {
            Id = state.Id,
            Name = state.Name,
            Abbreviation = state.Abbreviation,
            CountryId = state.CountryId,
            IsActive = state.IsActive,
            CreatedAtUtc = state.CreatedAtUtc,
            ModifiedAtUtc = state.ModifiedAtUtc,
            CreatedBy = state.CreatedBy,
            ModifiedBy = state.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this State state) where T : StateListResponse, new()
    {
        return new T
        {
            Id = state.Id,
            Name = state.Name,
            Abbreviation = state.Abbreviation,
            CountryId = state.CountryId,
            CountryName = state.Country?.Name,
            IsActive = state.IsActive
        };
    }
}