using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Operational.Persistence.Configurations.DateTimes;

public static class DateTimeConfiguration
{
    #region Conventions

    public static void ConfigureConvention(ModelConfigurationBuilder configurationBuilder, bool isNpgsql)
    {
        if (isNpgsql)
        {
            configurationBuilder.Properties<DateTimeOffset>()
                .HaveColumnType(DateTimeConstant.Types.Npgsql);

            configurationBuilder.Properties<DateTimeOffset?>()
                .HaveColumnType(DateTimeConstant.Types.Npgsql);
        }
        else
        {
            configurationBuilder.Properties<DateTimeOffset>()
                .HaveConversion<UtcDateTimeOffsetValueConverter>();

            configurationBuilder.Properties<DateTimeOffset?>()
                .HaveConversion<NullableUtcDateTimeOffsetValueConverter>();
        }
    }

    #endregion

    #region Property Mappings

    public static void ConfigureProperty<T>(ModelBuilder builder, Expression<Func<T, DateTimeOffset>> propertyExpression, bool isNpgsql) where T : class
    {
        PropertyBuilder<DateTimeOffset> propertyBuilder = builder.Entity<T>().Property(propertyExpression);

        if (isNpgsql)
            propertyBuilder.HasColumnType(DateTimeConstant.Types.Npgsql);
        else
            propertyBuilder.HasConversion<UtcDateTimeOffsetValueConverter>();
    }

    public static void ConfigureNullableProperty<T>(ModelBuilder builder, Expression<Func<T, DateTimeOffset?>> propertyExpression, bool isNpgsql) where T : class
    {
        PropertyBuilder<DateTimeOffset?> propertyBuilder = builder.Entity<T>().Property(propertyExpression);

        if (isNpgsql)
        {
            propertyBuilder.HasColumnType(DateTimeConstant.Types.Npgsql);
        }
        else
        {
            propertyBuilder.HasConversion<NullableUtcDateTimeOffsetValueConverter>();
        }
    }

    #endregion
}
