using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Extensions;
using Shared.Operational.Persistence.Specifications.Paging.Extensions;
using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Searching.Extensions;
using Shared.Operational.Persistence.Specifications.Sorting;
using Shared.Operational.Persistence.Specifications.Sorting.Extensions;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// Fluent <see cref="IQueryable{T}"/> extensions that apply filter, search, and sort
/// concerns from a <see cref="QueryingModel"/>.
/// </summary>
/// <remarks>
/// <para>
/// Methods return <see cref="IQueryable{T}"/> and should be followed by an explicit
/// pagination call from <c>PageModelEfCoreExtensions</c>. The standard handler pattern
/// is:
/// </para>
/// <code>
/// return await dbContext.Set&lt;T&gt;()
///     .AsNoTracking()
///     .ApplyQuerying(model, DefaultSort)
///     .ToPagedResultAsync(model.Page, ct);
/// </code>
/// <para>
/// Each concern is a no-op when its model is empty, so unused parameters never affect
/// the query. Delegates directly to the persistence-layer extension methods
/// (<c>FilterModelEfCoreExtensions.ApplyFilter</c>,
///  <c>SearchingModelQueryExtensions.ApplySearch</c>,
///  <c>SortingModelQueryExtensions.ApplySort</c>)
/// which already handle <c>IsEmpty</c> checks internally.
/// </para>
/// <para>
/// Terminal pagination overloads (<c>ToPagedResultAsync</c>, <c>ToPagedOrAllAsync</c>,
/// <c>ToPagedOrEmptyAsync</c>) accept <see cref="QueryingModel"/> as a convenience to
/// extract <c>model.Page</c> only — they do <b>not</b> apply filter/search/sort.
/// Call <c>ApplyQuerying</c> explicitly before paginating.
/// </para>
/// </remarks>
public static class QueryingModelExtensions
{
    #region Individual Concerns

    /// <summary>
    /// Applies the filter from a <see cref="QueryingModel"/> to the query.
    /// No-op when <see cref="FilterModel.IsEmpty"/> is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="model">The parsed querying model.</param>
    /// <returns>The query with the filter predicate applied, or the original query unchanged.</returns>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, QueryingModel model)
        => query.ApplyFilter(model.Filter);

    /// <summary>
    /// Applies the search from a <see cref="QueryingModel"/> to the query.
    /// No-op when <see cref="SearchModel.IsEmpty"/> is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="model">The parsed querying model.</param>
    /// <param name="defaultSearchFields">
    /// Entity-level default searchable fields. Used when <see cref="SearchModel.Fields"/> is empty.
    /// </param>
    /// <returns>The query with the search predicate applied, or the original query unchanged.</returns>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        QueryingModel model,
        IReadOnlyList<string>? defaultSearchFields = null) where T : class
        => query.ApplySearch(model.Search, defaultSearchFields);

    /// <summary>
    /// Applies the sort from a <see cref="QueryingModel"/> to the query.
    /// No-op when <see cref="SortModel.IsEmpty"/> is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="model">The parsed querying model.</param>
    /// <param name="defaultClauses">
    /// Entity-level default sort clauses. Used when <see cref="SortModel.Clauses"/> is empty.
    /// </param>
    /// <returns>The query with the sort applied, or the original query unchanged.</returns>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        QueryingModel model,
        IReadOnlyList<SortClause>? defaultClauses = null) where T : class
        => query.ApplySort(model.Sort, defaultClauses);

    #endregion Individual Concerns

    #region Composed — Filter + Search + Sort (no page)

    /// <summary>
    /// Applies filter, search, and sort from a <see cref="QueryingModel"/> in sequence,
    /// leaving pagination to the caller.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="model">The parsed querying model.</param>
    /// <param name="defaultSearchFields">
    /// Entity-level default searchable fields. Used when <see cref="SearchModel.Fields"/>
    /// is empty.
    /// </param>
    /// <param name="defaultSortClauses">
    /// Entity-level default sort clauses. Used when <see cref="SortModel.Clauses"/> is empty.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with filter, search, and sort applied.
    /// Follow with a pagination call from <c>PageModelEfCoreExtensions</c>.
    /// </returns>
    public static IQueryable<T> ApplyQuerying<T>(
        this IQueryable<T> query,
        QueryingModel model,
        IReadOnlyList<string>? defaultSearchFields = null,
        IReadOnlyList<SortClause>? defaultSortClauses = null) where T : class
        => query
            .ApplyFilter(model)
            .ApplySearch(model, defaultSearchFields)
            .ApplySort(model, defaultSortClauses);

    #endregion Composed — Filter + Search + Sort (no page)

    #region Terminal — Pagination Only (Without Projection)

    /// <summary>
    /// Paginates the query using the page from <paramref name="model"/>.
    /// Does <b>not</b> apply filter/search/sort — call <see cref="ApplyQuerying{T}"/>
    /// explicitly before this method.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="model">The querying model from which <see cref="QueryingModel.Page"/> is extracted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{T}"/> with items and pagination metadata.</returns>
    public static Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        QueryingModel model,
        CancellationToken ct = default) where T : class
        => query.ToPagedResultAsync(model.Page, ct);

    /// <summary>
    /// Returns all items when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise paginates. Does <b>not</b> apply filter/search/sort.
    /// </summary>
    public static Task<PagedResult<T>> ToPagedOrAllAsync<T>(
        this IQueryable<T> query,
        QueryingModel model,
        CancellationToken ct = default) where T : class
        => query.ToPagedOrAllAsync(model.Page, ct);

    /// <summary>
    /// Returns an empty result when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise paginates. Does <b>not</b> apply filter/search/sort.
    /// </summary>
    public static Task<PagedResult<T>> ToPagedOrEmptyAsync<T>(
        this IQueryable<T> query,
        QueryingModel model,
        CancellationToken ct = default) where T : class
        => query.ToPagedOrEmptyAsync(model.Page, ct);

    #endregion Terminal — Pagination Only (Without Projection)

    #region Terminal — Pagination Only (With Projection)

    /// <summary>
    /// Paginates the query with a projection using the page from <paramref name="model"/>.
    /// Does <b>not</b> apply filter/search/sort — call <see cref="ApplyQuerying{T}"/>
    /// explicitly before this method.
    /// </summary>
    /// <typeparam name="TSource">The entity type.</typeparam>
    /// <typeparam name="TDestination">The projected response type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="model">The querying model from which <see cref="QueryingModel.Page"/> is extracted.</param>
    /// <param name="projection">The EF Core projection expression.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{TDestination}"/> with items and pagination metadata.</returns>
    public static Task<PagedResult<TDestination>> ToPagedResultAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        QueryingModel model,
        Expression<Func<TSource, TDestination>> projection,
        CancellationToken ct = default) where TSource : class
        => query.ToPagedResultAsync(projection, model.Page, ct);

    /// <summary>
    /// Returns all items when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise paginates with a projection. Does <b>not</b> apply filter/search/sort.
    /// </summary>
    public static Task<PagedResult<TDestination>> ToPagedOrAllAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        QueryingModel model,
        Expression<Func<TSource, TDestination>> projection,
        CancellationToken ct = default) where TSource : class
        => query.ToPagedOrAllAsync(projection, model.Page, ct);

    /// <summary>
    /// Returns an empty result when <see cref="PageModel.IsEmpty"/> is <see langword="true"/>,
    /// otherwise paginates with a projection. Does <b>not</b> apply filter/search/sort.
    /// </summary>
    public static Task<PagedResult<TDestination>> ToPagedOrEmptyAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        QueryingModel model,
        Expression<Func<TSource, TDestination>> projection,
        CancellationToken ct = default) where TSource : class
        => query.ToPagedOrEmptyAsync(projection, model.Page, ct);

    #endregion Terminal — Pagination Only (With Projection)
}
