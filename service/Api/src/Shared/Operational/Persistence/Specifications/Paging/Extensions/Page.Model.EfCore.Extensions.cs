using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace Shared.Operational.Persistence.Specifications.Paging.Extensions;

/// <summary>
/// EF Core extension methods for paginating <see cref="IQueryable{T}"/> queries
/// using a parsed <see cref="PageModel"/>.
/// </summary>
public static class PageModelEfCoreExtensions
{
    #region Async — With Projection

    /// <summary>
    /// Executes a projected query and returns a paged result asynchronously.
    /// Always paginates regardless of <see cref="PageModel.IsEmpty"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source entity.</typeparam>
    /// <typeparam name="TDestination">The type of the projected destination model.</typeparam>
    /// <param name="query">The <see cref="IQueryable{TSource}"/> to paginate.</param>
    /// <param name="projection">The projection expression applied after pagination.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TDestination}"/> containing items and metadata.</returns>
    public static async Task<PagedResult<TDestination>> ToPagedResultAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        PageModel page,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        // Await: Get total count for pagination metadata.
        long count = await query.LongCountAsync(cancellationToken);

        // Await: Fetch the specific page of items with projection.
        List<TDestination> items = await query
            .AsNoTracking()
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(projection)
            .ToListAsync(cancellationToken);

        // Create: Paged result with metadata.
        return PagedResult<TDestination>.Create(items, page.Page, page.PageSize, count);
    }

    /// <summary>
    /// Returns all items when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise returns a paged result. Use for endpoints where pagination is optional.
    /// </summary>
    /// <typeparam name="TSource">The type of the source entity.</typeparam>
    /// <typeparam name="TDestination">The type of the projected destination model.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="projection">The projection expression.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TDestination}"/>.</returns>
    public static async Task<PagedResult<TDestination>> ToPagedOrAllAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        PageModel page,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (page.IsEmpty)
        {
            // Await: Fetch all items — caller passed no pagination parameters.
            List<TDestination> allItems = await query
                .AsNoTracking()
                .Select(projection)
                .ToListAsync(cancellationToken);

            return PagedResult<TDestination>.Create(
                items:      allItems,
                page:       1,
                pageSize:   Math.Max(1, allItems.Count),
                totalCount: allItems.Count);
        }

        return await query.ToPagedResultAsync(projection, page, cancellationToken);
    }

    /// <summary>
    /// Returns an empty result when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise returns a paged result. Use for endpoints where pagination is mandatory.
    /// </summary>
    /// <typeparam name="TSource">The type of the source entity.</typeparam>
    /// <typeparam name="TDestination">The type of the projected destination model.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="projection">The projection expression.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TDestination}"/>.</returns>
    public static async Task<PagedResult<TDestination>> ToPagedOrEmptyAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        PageModel page,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (page.IsEmpty)
            return PagedResult<TDestination>.NoContent();

        return await query.ToPagedResultAsync(projection, page, cancellationToken);
    }

    #endregion Async — With Projection

    #region Async — Without Projection

    /// <summary>
    /// Executes a query and returns a paged result asynchronously.
    /// Always paginates regardless of <see cref="PageModel.IsEmpty"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TSource}"/>.</returns>
    public static async Task<PagedResult<TSource>> ToPagedResultAsync<TSource>(
        this IQueryable<TSource> query,
        PageModel page,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        // Await: Get total count for pagination metadata.
        long count = await query.LongCountAsync(cancellationToken);

        // Await: Fetch the specific page of items.
        List<TSource> items = await query
            .AsNoTracking()
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        // Create: Paged result with metadata.
        return PagedResult<TSource>.Create(items, page.Page, page.PageSize, count);
    }

    /// <summary>
    /// Returns all items when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise returns a paged result.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TSource}"/>.</returns>
    public static async Task<PagedResult<TSource>> ToPagedOrAllAsync<TSource>(
        this IQueryable<TSource> query,
        PageModel page,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (page.IsEmpty)
        {
            // Await: Fetch all items — caller passed no pagination parameters.
            List<TSource> allItems = await query.AsNoTracking().ToListAsync(cancellationToken);

            return PagedResult<TSource>.Create(
                items:      allItems,
                page:       1,
                pageSize:   Math.Max(1, allItems.Count),
                totalCount: allItems.Count);
        }

        return await query.ToPagedResultAsync(page, cancellationToken);
    }

    /// <summary>
    /// Returns an empty result when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise returns a paged result.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="page">The normalized pagination model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TSource}"/>.</returns>
    public static async Task<PagedResult<TSource>> ToPagedOrEmptyAsync<TSource>(
        this IQueryable<TSource> query,
        PageModel page,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (page.IsEmpty)
            return PagedResult<TSource>.NoContent();

        return await query.ToPagedResultAsync(page, cancellationToken);
    }

    #endregion Async — Without Projection
}
