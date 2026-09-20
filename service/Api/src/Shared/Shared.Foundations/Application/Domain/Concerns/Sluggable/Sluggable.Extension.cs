using Shared.Application.Domain.Concerns.Entities;

namespace Shared.Application.Domain.Concerns.Sluggable;

/// <summary>
/// Provides LINQ extension methods for querying ISluggable entities.
/// </summary>
public static class SluggableExtensions
{
    /// <summary>
    /// Filters the query to entities with a matching slug.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The query to filter.</param>
    /// <param name="slug">The slug to match.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> WhereSlugEquals<T>(this IQueryable<T> query, string slug)
        where T : ISluggable
    {
        // Filter: Exact slug match on sluggable entity
        return query.Where(x => x.Slug == slug);
    }

    public static IQueryable<T> WhereDuplicateSlug<T, TId>(
        this IQueryable<T> query,
        string slug,
        TId? excludeId = default)
        where T : ISluggable, IEntity<TId>
    {
        IQueryable<T> filteredQuery = query.WhereSlugEquals(slug);

        if (excludeId != null && !excludeId.Equals(default(TId)))
        {
            filteredQuery = filteredQuery.Where(x => !x.Id!.Equals(excludeId));
        }

        return filteredQuery;
    }
}
