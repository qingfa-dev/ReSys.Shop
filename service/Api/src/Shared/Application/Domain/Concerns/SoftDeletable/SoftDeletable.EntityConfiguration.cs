using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Application.Domain.Concerns.SoftDeletable;

/// <summary>
/// Configures soft delete properties and global query filters for entities.
/// </summary>
public static class SoftDeletableConfiguration
{
    /// <summary>
    /// Applies soft delete configurations to the entity type builder.
    /// Expects the entity type to implement <see cref="ISoftDeletable"/>.
    /// </summary>
    public static void Apply<T>(EntityTypeBuilder builder) where T : class, ISoftDeletable
    {
        Type type = builder.Metadata.ClrType;

        builder.HasQueryFilter(CreateSoftDeletableFilter(type));
        builder.Property(nameof(ISoftDeletable.IsDeleted)).IsRequired();
        builder.Property(nameof(ISoftDeletable.DeletedAtUtc));
        builder.Property(nameof(ISoftDeletable.DeletedBy)).HasMaxLength(100);
    }

    private static LambdaExpression CreateSoftDeletableFilter(Type type)
    {
        ParameterExpression parameter = Expression.Parameter(type, "e");
        MemberExpression property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        UnaryExpression notExpression = Expression.Not(property);
        return Expression.Lambda(notExpression, parameter);
    }
}