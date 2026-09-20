using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Operational.Persistence.Configurations.Enums;

/// <summary>
/// Configures enum persistence across database providers.
///
/// PostgreSQL:
/// - Uses native PostgreSQL enum types.
/// - Generates CREATE TYPE migrations.
/// - Does not apply string conversions.
///
/// Other providers:
/// - Stores enums as strings using <see cref="EnumToStringConverter{TEnum}"/>.
/// </summary>
public static class EnumConfiguration
{
    #region Conventions

    /// <summary>
    /// Configures global enum conventions.
    ///
    /// For non-PostgreSQL providers, all enums are stored as strings.
    /// PostgreSQL uses native enum types and therefore skips value conversion.
    /// </summary>
    /// <param name="configurationBuilder">
    /// EF Core convention builder.
    /// </param>
    /// <param name="isNpgsql">
    /// Indicates whether the current provider is PostgreSQL.
    /// </param>
    public static void ConfigureConvention(
        ModelConfigurationBuilder configurationBuilder,
        bool isNpgsql = false)
    {
        if (isNpgsql)
        {
            return;
        }

        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }

    #endregion

    #region Model Configuration

    /// <summary>
    /// Configures provider-specific enum mappings.
    ///
    /// For PostgreSQL, all discovered enum types are registered
    /// using <c>HasPostgresEnum&lt;TEnum&gt;</c> so migrations generate
    /// native PostgreSQL enum definitions.
    /// </summary>
    /// <param name="modelBuilder">
    /// EF Core model builder.
    /// </param>
    /// <param name="isNpgsql">
    /// Indicates whether the current provider is PostgreSQL.
    /// </param>
    public static void ConfigureModel(
        ModelBuilder modelBuilder,
        bool isNpgsql = false)
    {
        if (!isNpgsql)
        {
            return;
        }

        IEnumerable<Type> enumTypes = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(x => x.GetProperties())
            .Select(x => Nullable.GetUnderlyingType(x.ClrType) ?? x.ClrType)
            .Where(x => x.IsEnum)
            .Distinct();

        foreach (Type? enumType in enumTypes)
        {
            RegisterPostgresEnum(modelBuilder, enumType);
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Registers a PostgreSQL enum using reflection.
    /// </summary>
    private static void RegisterPostgresEnum(
        ModelBuilder modelBuilder,
        Type enumType)
    {
        RegisterPostgresEnumMethod
            .MakeGenericMethod(enumType)
            .Invoke(null, [modelBuilder]);
    }

    /// <summary>
    /// Registers a PostgreSQL enum type in the EF Core model.
    /// </summary>
    private static void RegisterPostgresEnumGeneric<TEnum>(
        ModelBuilder modelBuilder)
        where TEnum : struct, Enum
    {
        modelBuilder.HasPostgresEnum<TEnum>();
    }

    private static readonly MethodInfo RegisterPostgresEnumMethod =
        typeof(EnumConfiguration)
            .GetMethod(
                nameof(RegisterPostgresEnumGeneric),
                BindingFlags.NonPublic | BindingFlags.Static)!;

    #endregion
}