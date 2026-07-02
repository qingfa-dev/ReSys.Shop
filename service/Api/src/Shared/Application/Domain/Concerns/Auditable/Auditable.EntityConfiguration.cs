using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Application.Domain.Concerns.Auditable;

/// <summary>
/// Configures auditing properties for entities.
/// </summary>
public static class AuditableConfiguration
{
    /// <summary>
    /// Applies auditing configurations to the entity type builder.
    /// Expects the entity type to implement <see cref="IAuditable"/>.
    /// </summary>
    public static void Apply<T>(EntityTypeBuilder<T> builder)
        where T : class, IAuditable
    {
        builder.Property(m => m.CreatedAtUtc)
            .IsRequired();

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(AuditableConstant.Constraints.MaxCreatedByLength);

        builder.Property(m => m.ModifiedAtUtc);

        builder.Property(m => m.ModifiedBy)
            .HasMaxLength(AuditableConstant.Constraints.MaxModifiedByLength);
    }
}
