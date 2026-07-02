using Microsoft.AspNetCore.Mvc;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// Defines the flat query-string surface for a filter expression.
/// </summary>
/// <remarks>
/// Implemented by any request DTO that accepts filter input from the HTTP layer.
/// The single <see cref="Filter"/> string uses the Querying DSL:
/// <list type="bullet">
///   <item><description>AND via comma: <c>Name=*bolt*,IsDeleted=false</c></description></item>
///   <item><description>OR via pipe: <c>Status=Active|Status=Pending</c></description></item>
///   <item><description>Grouping via parentheses: <c>(Status=Active|Status=Pending),IsDeleted=false</c></description></item>
///   <item><description>All operators: <c>=</c> <c>!=</c> <c>*</c> <c>!*</c> <c>^</c> <c>$</c> <c>&gt;</c> <c>&gt;=</c> <c>&lt;</c> <c>&lt;=</c> and case-sensitive variants <c>==</c> <c>*~</c> <c>^~</c> <c>$~</c></description></item>
/// </list>
/// Parsed by <c>FilterModelExtensions.FromString</c> in the handler.
/// </remarks>
public interface IFilteringParameters
{
    /// <summary>
    /// Gets the DSL filter string, or <see langword="null"/> when no filter was requested.
    /// Example: <c>Name=*bolt*, IsDeleted=false</c>
    /// </summary>
    [FromQuery(Name = "filter")]
    string? Filter { get; }
}
