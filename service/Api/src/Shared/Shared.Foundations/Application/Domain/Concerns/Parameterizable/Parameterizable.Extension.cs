using Shared.Application.Domain.Concerns.Entities;

namespace Shared.Application.Domain.Concerns.Parameterizable;

/// <summary>
/// Provides LINQ extension methods for querying IParameterizable entities.
/// </summary>
public static class ParameterizableExtensions
{
    public static IQueryable<T> WhereNameEquals<T>(this IQueryable<T> query, string name)
        where T : IParameterizable
    {
        // Filter: Exact name match on parameterizable entity
        return query.Where(x => x.Name == name);
    }

    public static IQueryable<T> WhereNotId<T, TId>(this IQueryable<T> query, TId id)
        where T : IEntity<TId>
    {
        return query.Where(x => !x.Id!.Equals(id));
    }

    public static IQueryable<T> WhereDuplicateName<T, TId>(
        this IQueryable<T> query,
        string name,
        TId? excludeId = default)
        where T : IParameterizable, IEntity<TId>
    {
        IQueryable<T> filteredQuery = query.WhereNameEquals(name);

        if (excludeId != null && !excludeId.Equals(default(TId)))
        {
            filteredQuery = filteredQuery.WhereNotId(excludeId);
        }

        return filteredQuery;
    }
}
