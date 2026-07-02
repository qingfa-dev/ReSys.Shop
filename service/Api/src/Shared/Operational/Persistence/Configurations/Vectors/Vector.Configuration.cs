using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Pgvector;

namespace Shared.Operational.Persistence.Configurations.Vectors;

public static class VectorConfiguration
{
    #region Conventions

    public static void ConfigureConvention(ModelConfigurationBuilder configurationBuilder, bool isNpgsql)
    {
        if (!isNpgsql)
        {
            configurationBuilder.Properties<Vector>()
                .HaveConversion<VectorValueConverter>();
        }
    }

    #endregion

    #region Model Configuration

    public static void ConfigureModel(ModelBuilder builder, bool isNpgsql)
    {
        if (isNpgsql)
        {
            builder.HasPostgresExtension("vector");
        }
    }

    #endregion

    #region Property Mappings

    public static void ConfigureProperty<T>(ModelBuilder builder, Expression<Func<T, Vector>> propertyExpression, bool isNpgsql) where T : class
    {
        if (!isNpgsql)
        {
            builder.Entity<T>()
                .Property(propertyExpression)
                .HasConversion<VectorValueConverter>();
        }
    }

    #endregion
}
