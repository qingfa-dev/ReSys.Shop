using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.OptionTypes.Values;

/// <summary>
/// Represents a specific value within an option type (e.g., "Small", "Red").
/// </summary>
// Invariant: Name != null; Position >= -1; OptionTypeId != Guid.Empty
public partial class OptionValue : Entity, IAuditable, IParameterizable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Presentation { get; set; } = string.Empty;
    public int Position { get; set; }
    #endregion Properties

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Relationship
    public Guid OptionTypeId { get; set; }
    public OptionType OptionType { get; set; } = null!;
    #endregion Relationship

    #region Constructor
    internal OptionValue() { }
    #endregion Constructor
}
