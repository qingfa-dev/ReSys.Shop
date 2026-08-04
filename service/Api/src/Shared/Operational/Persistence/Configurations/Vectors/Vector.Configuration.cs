using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

    #region Vector Indexes

    public static IndexBuilder<T> ConfigureIVFFlatIndex<T>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, Vector?>> propertyExpression,
        int lists = 100,
        string? indexName = null) where T : class
    {
        IndexBuilder<T> indexBuilder = builder.HasIndex(ToObjectExpression(propertyExpression));
        indexBuilder.HasMethod("ivfflat");
        indexBuilder.HasOperators("vector_cosine_ops");
        indexBuilder.HasStorageParameter("lists", lists);
        if (indexName is not null)
            indexBuilder.HasDatabaseName(indexName);
        return indexBuilder;
    }

    public static IndexBuilder<T> ConfigureHNSWIndex<T>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, Vector?>> propertyExpression,
        int m = 16,
        int efConstruction = 200,
        string? indexName = null) where T : class
    {
        IndexBuilder<T> indexBuilder = builder.HasIndex(ToObjectExpression(propertyExpression));
        indexBuilder.HasMethod("hnsw");
        indexBuilder.HasOperators("vector_cosine_ops");
        indexBuilder.HasStorageParameter("m", m);
        indexBuilder.HasStorageParameter("ef_construction", efConstruction);
        if (indexName is not null)
            indexBuilder.HasDatabaseName(indexName);
        return indexBuilder;
    }

    private static Expression<Func<T, object?>> ToObjectExpression<T>(Expression<Func<T, Vector?>> expression)
    {
        return Expression.Lambda<Func<T, object?>>(
            Expression.Convert(expression.Body, typeof(object)),
            expression.Parameters);
    }

    #endregion
}
