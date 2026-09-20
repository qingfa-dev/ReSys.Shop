using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Filtering.Expression;
using Shared.Operational.Persistence.Specifications.Helpers;

namespace Shared.Operational.Persistence.Specifications.Filtering.Extensions;

/// <summary>
/// EF Core integration extensions that bridge <see cref="FilterModel"/> into
/// <see cref="IQueryable{T}"/> predicate application.
/// </summary>
/// <remarks>
/// These extensions sit at the boundary between the model layer (which knows nothing about
/// EF Core) and the querying layer (which knows nothing about the model structure).
/// <para>
/// <b>Boundary:</b> The <see cref="FilterModel"/> is walked by
/// <see cref="FilterGroupVisitor{T}"/> to build Expression trees directly,
/// avoiding a redundant roundtrip through the DSL string. Expression compilation
/// is cached via <see cref="QueryHelper.GetCachedExpression{T}"/>.
/// </para>
/// </remarks>
public static class FilterModelEfCoreExtensions
{
    #region IQueryable — with FilterModel

    /// <summary>
    /// Applies a parsed <see cref="FilterModel"/> to the query by walking the
    /// <see cref="FilterGroup"/> tree via <see cref="FilterGroupVisitor{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source <see cref="IQueryable{T}"/>.</param>
    /// <param name="model">
    /// The <see cref="FilterModel"/> to apply. When <see langword="null"/>, empty, or invalid,
    /// the original query is returned unchanged (fail-safe).
    /// </param>
    /// <returns>A filtered <see cref="IQueryable{T}"/>, or the original query if the model
    /// is null, empty, or invalid.</returns>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        FilterModel? model)
    {
        // Guard: Null / empty / invalid models are no-ops.
        if (model is null || model.IsEmpty || !model.IsValid) return query;

        // Cache: Build a structural key from the group tree to avoid collisions
        // when different nestings produce the same flat DSL string.
        string cacheKey = model.Root.ToStructuralKey();

        LambdaExpression? cachedLambda = QueryHelper.GetCachedExpression<T>(cacheKey, FilterModelConstant.Cache.ModelPrefix, () =>
        {
            try
            {
                ParameterExpression param = System.Linq.Expressions.Expression.Parameter(typeof(T), FilterModelConstant.Expression.ParameterName);
                System.Linq.Expressions.Expression? body = FilterGroupVisitor<T>.Build(model.Root, param);
                return body == null ? null : System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
            }
            catch
            {
                return null;
            }
        });

        if (cachedLambda == null) return query;
        return cachedLambda is Expression<Func<T, bool>> typedLambda ? query.Where(typedLambda) : query;
    }

    /// <summary>
    /// Applies a <see cref="Result{FilterModel}"/> to the query.
    /// Failed or empty results leave the query unchanged.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source <see cref="IQueryable{T}"/>.</param>
    /// <param name="modelResult">The parse result wrapping a <see cref="FilterModel"/>.</param>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        Result<FilterModel> modelResult)
        => modelResult.IsFailure
            ? query
            : query.ApplyFilter(modelResult.Value);

    #endregion IQueryable — with FilterModel

    #region IQueryable — convenience from raw input

    /// <summary>
    /// Parses a DSL filter string and applies it to the query in one step.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source <see cref="IQueryable{T}"/>.</param>
    /// <param name="filterString">The raw DSL filter string.</param>
    /// <param name="allowedFields">Optional whitelist of field names.</param>
    public static IQueryable<T> ApplyFilterString<T>(
        this IQueryable<T> query,
        string? filterString,
        string[]? allowedFields = null)
    {
        Result<FilterModel> result = FilterModelExtensions.FromString(filterString, allowedFields);
        return query.ApplyFilter(result);
    }

    /// <summary>
    /// Parses a JSON filter payload and applies it to the query in one step.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source <see cref="IQueryable{T}"/>.</param>
    /// <param name="json">The raw JSON string.</param>
    /// <param name="allowedFields">Optional whitelist of field names.</param>
    public static IQueryable<T> ApplyFilterJson<T>(
        this IQueryable<T> query,
        string? json,
        string[]? allowedFields = null)
    {
        Result<FilterModel> result = FilterModelExtensions.FromJson(json, allowedFields);
        return query.ApplyFilter(result);
    }

    /// <summary>
    /// Parses query-string triplets and applies them to the query in one step.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source <see cref="IQueryable{T}"/>.</param>
    /// <param name="values">The raw <c>field:op:value</c> triplet strings.</param>
    /// <param name="allowedFields">Optional whitelist of field names.</param>
    public static IQueryable<T> ApplyFilterQueryString<T>(
        this IQueryable<T> query,
        IEnumerable<string?>? values,
        string[]? allowedFields = null)
    {
        Result<FilterModel> result = FilterModelExtensions.FromQueryString(values, allowedFields);
        return query.ApplyFilter(result);
    }

    #endregion IQueryable — convenience from raw input
}
