using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.Application.Domain.Concerns.Entities;

/// <summary>
/// Configures auditing properties for entities.
/// </summary>
public static class EntityConfiguration
{
    /// <summary>
    /// Applies auditing configurations to the entity type builder.
    /// Expects the entity type to implement <see cref="IAuditable"/>.
    /// </summary>
    public static void Apply<T>(EntityTypeBuilder<T> builder)
        where T : class, IEntity
    {
        Type type = builder.Metadata.ClrType;

        // Check: Ensure all entities have Id property as primary key
        builder.HasKey(nameof(IEntity.Id));

        // Initialize: Configure Guid IDs (IEntity) to be client-side generated
        if (typeof(IEntity).IsAssignableFrom(type))
        {
            builder.Property(nameof(IEntity.Id)).ValueGeneratedNever();
        }
    }
}
