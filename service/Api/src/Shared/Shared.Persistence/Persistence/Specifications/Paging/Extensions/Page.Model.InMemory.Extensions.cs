namespace Shared.Operational.Persistence.Specifications.Paging.Extensions;

/// <summary>
/// In-memory extension methods for paginating <see cref="IEnumerable{T}"/> collections
/// using a parsed <see cref="PageModel"/>.
/// </summary>
public static class PageModelInMemoryExtensions
{
    #region Synchronous — In-Memory

    /// <summary>
    /// Paginates an in-memory collection with projection.
    /// Always paginates regardless of <see cref="PageModel.IsEmpty"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of source elements.</typeparam>
    /// <typeparam name="TDestination">The type of projected elements.</typeparam>
    /// <param name="source">The collection to paginate.</param>
    /// <param name="projection">The projection function.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <returns>A <see cref="PagedResult{TDestination}"/>.</returns>
    public static PagedResult<TDestination> ToPagedResult<TSource, TDestination>(
        this IEnumerable<TSource> source,
        Func<TSource, TDestination> projection,
        PageModel page)
    {
        // Materialize: Avoid double-enumeration.
        IList<TSource> list = source as IList<TSource> ?? source.ToList();
        long count = list.Count;

        // Transform: Extract and project the page items from memory.
        List<TDestination> items = list
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(projection)
            .ToList();

        // Create: Paged result with metadata.
        return PagedResult<TDestination>.Create(items, page.Page, page.PageSize, count);
    }

    /// <summary>
    /// Paginates an in-memory collection.
    /// Always paginates regardless of <see cref="PageModel.IsEmpty"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The collection to paginate.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <returns>A <see cref="PagedResult{T}"/>.</returns>
    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source,
        PageModel page)
    {
        // Materialize: Avoid double-enumeration.
        IList<T> list = source as IList<T> ?? source.ToList();
        long count = list.Count;

        // Transform: Extract the page items from memory.
        List<T> items = list
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToList();

        // Create: Paged result with metadata.
        return PagedResult<T>.Create(items, page.Page, page.PageSize, count);
    }

    #endregion Synchronous — In-Memory
}
