using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Entities;
using Shared.Application.Domain.Concerns.Sluggable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Concerns.Versionable;

namespace Shared.Operational.Persistence.Configurations.Models;

public static class EntityModelConfiguration
{
    #region Model Configuration
    // Cache the generic ModelBuilder.Entity<T>() method once to avoid repeated LINQ searches.
    private static readonly MethodInfo GenericEntityMethod = typeof(ModelBuilder)
        .GetMethods()
        .First(m => m.Name == nameof(ModelBuilder.Entity)
                    && m.IsGenericMethod
                    && m.GetParameters().Length == 0);
    public static void ConfigureModel(ModelBuilder modelBuilder, bool isNpgsql = false)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            Type clrType = entityType.ClrType;
            Type[] interfaces = clrType.GetInterfaces();

            // Check: Verify if entity implements IEntity for key configuration
            bool isEntity = interfaces.Any(i =>
                i == typeof(IEntity) ||
                (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)));

            // Check: Determine if entity needs generic builder for interface configurations
            bool needsGenericBuilder =
                interfaces.Contains(typeof(IEntity)) ||
                interfaces.Contains(typeof(IVersionable)) ||
                interfaces.Contains(typeof(IAuditable)) ||
                interfaces.Contains(typeof(ISoftDeletable)) ||
                interfaces.Contains(typeof(ISluggable));

            if (needsGenericBuilder)
            {
                object genericBuilder = GenericEntityMethod
                    .MakeGenericMethod(clrType)
                    .Invoke(modelBuilder, null)!;

                // Call: Apply key configuration via EntityConfiguration
                if (interfaces.Contains(typeof(IEntity)))
                    Invoke(typeof(EntityConfiguration), clrType, genericBuilder);

                // Call: Apply optimistic concurrency configuration via VersionableConfiguration
                if (interfaces.Contains(typeof(IVersionable)))
                    Invoke(typeof(VersionableConfiguration), clrType, genericBuilder);

                // Call: Apply audit column configuration via AuditableConfiguration
                if (interfaces.Contains(typeof(IAuditable)))
                    Invoke(typeof(AuditableConfiguration), clrType, genericBuilder);

                // Call: Apply soft delete and query filter via SoftDeletableConfiguration
                if (interfaces.Contains(typeof(ISoftDeletable)))
                    Invoke(typeof(SoftDeletableConfiguration), clrType, genericBuilder);

                // Call: Apply unique slug index configuration via SluggableConfiguration
                if (interfaces.Contains(typeof(ISluggable)))
                    Invoke(typeof(SluggableConfiguration), clrType, genericBuilder);
            }
        }
    }

    #endregion

    #region Helpers
    // -------------------------------------------------------------------------
    // Transform: Call ConfigurationClass.Apply<TEntity>(builder) via reflection
    // -------------------------------------------------------------------------
    private static void Invoke(Type configurationType, Type entityType, object builder)
    {
        configurationType
            .GetMethod("Apply")!
            .MakeGenericMethod(entityType)
            .Invoke(null, [builder]);
    }
    #endregion
}
