namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Specifies the logical connective applied between the members of a <see cref="FilterGroup"/>.
/// </summary>
public enum FilterLogic
{
    /// <summary>All conditions and sub-groups within the group must match.</summary>
    And,

    /// <summary>At least one condition or sub-group within the group must match.</summary>
    Or,
}