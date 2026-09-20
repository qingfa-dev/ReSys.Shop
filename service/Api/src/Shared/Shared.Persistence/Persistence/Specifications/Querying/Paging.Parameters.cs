using Microsoft.AspNetCore.Mvc;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// Defines the flat query-string surface for a pagination request.
/// </summary>
/// <remarks>
/// Implemented by any request DTO that accepts pagination input from the HTTP layer.
/// Both values are nullable integers — <c>[AsParameters]</c> binds query-string values
/// into nullable integers directly via model binding.
/// <para>
/// When both <see cref="PageNumber"/> and <see cref="PageSize"/> are absent, the handler receives
/// a <c>PageModel</c> with <c>IsEmpty = true</c>. The paging extension methods
/// (<c>ToPagedOrAllAsync</c> / <c>ToPagedOrEmptyAsync</c>) use this flag to decide
/// whether to skip pagination entirely.
/// </para>
/// </remarks>
public interface IPagingParameters
{
    /// <summary>
    /// Gets the page number, or <see langword="null"/> when not supplied.
    /// Out-of-range values are clamped to ≥ 1 by <c>PageBounds</c>.
    /// Example: <c>?page=2</c>
    /// </summary>
    [FromQuery(Name = "page")]
    int? PageNumber { get; }

    /// <summary>
    /// Gets the page size, or <see langword="null"/> when not supplied.
    /// Out-of-range values are clamped to [1, <c>PageBounds.MaxPageSize</c>].
    /// Example: <c>?pageSize=25</c>
    /// </summary>
    [FromQuery(Name = "pageSize")]
    int? PageSize { get; }
}
