namespace Shared.Operational.Persistence.Specifications.Sorting;

/// <summary>
/// Specifies where <see langword="null"/> values appear when sorting a nullable field.
/// </summary>
/// <remarks>
/// Recognized JSON / query-string aliases: <c>"first"</c>, <c>"last"</c>.
/// When absent, the database engine's default ordering for nulls applies.
/// </remarks>
public enum SortNulls
{
    /// <summary>Null values appear before non-null values.</summary>
    First,

    /// <summary>Null values appear after non-null values.</summary>
    Last
}
