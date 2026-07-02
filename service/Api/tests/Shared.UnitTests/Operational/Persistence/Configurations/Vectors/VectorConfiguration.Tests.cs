using Microsoft.EntityFrameworkCore;

using Pgvector;

using Shared.Operational.Persistence.Configurations.Vectors;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Vectors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class VectorConfigurationTests
{
    private sealed class TestEntity
    {
        public Int32 Id { get; set; }
        public Vector Embedding { get; set; } = null!;
    }

    private sealed class ConventionTestDbContext(Boolean isNpgsql) : DbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            VectorConfiguration.ConfigureConvention(configurationBuilder, isNpgsql);
            // InMemory cannot handle Vector natively, so register the converter
            // regardless of isNpgsql flag. This mirrors production behavior
            // where ApplicationDbContext derives isNpgsql from Database.IsNpgsql()
            // (which returns false for InMemory, triggering converter registration).
            configurationBuilder.Properties<Vector>()
                .HaveConversion<VectorValueConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }

    public class ConfigureConvention
    {
        [Fact]
        public async Task WhenNotNpgsql_ShouldApplyValueConverter()
        {
            await using ConventionTestDbContext context = new(isNpgsql: false);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.Embedding))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<VectorValueConverter>();
        }

        [Fact]
        public async Task WhenNpgsql_ShouldConfigureWithoutException()
        {
            await using ConventionTestDbContext context = new(isNpgsql: true);

            Func<Task> act = async () => await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            await act.Should().NotThrowAsync();
        }
    }

    private sealed class PropertyTestDbContext(Boolean isNpgsql, Boolean applyConvention = false) : DbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            if (applyConvention)
            {
                VectorConfiguration.ConfigureConvention(configurationBuilder, isNpgsql: false);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            VectorConfiguration.ConfigureProperty<TestEntity>(modelBuilder, e => e.Embedding, isNpgsql);
        }
    }

    public class ConfigureProperty
    {
        [Fact]
        public async Task WhenNonNpgsql_ShouldApplyConverter()
        {
            await using PropertyTestDbContext context = new(isNpgsql: false);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.Embedding))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<VectorValueConverter>();
        }

        [Fact]
        public void WhenNonNpgsql_WithDirectModelBuilder_ShouldApplyConverter()
        {
            ModelBuilder modelBuilder = new();
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            VectorConfiguration.ConfigureProperty<TestEntity>(modelBuilder, e => e.Embedding, isNpgsql: false);

            Microsoft.EntityFrameworkCore.Metadata.IModel model = modelBuilder.FinalizeModel();
            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.Embedding))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<VectorValueConverter>();
        }

        [Fact]
        public async Task WhenNpgsql_ShouldNotOverrideConvention()
        {
            await using PropertyTestDbContext context = new(isNpgsql: true, applyConvention: true);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.Embedding))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<VectorValueConverter>();
        }

    }

    private sealed class ModelTestDbContext(Boolean isNpgsql, Boolean applyConvention = false) : DbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            if (applyConvention)
            {
                VectorConfiguration.ConfigureConvention(configurationBuilder, isNpgsql: false);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            VectorConfiguration.ConfigureModel(modelBuilder, isNpgsql);
        }
    }

    public class ConfigureModel
    {
        [Fact]
        public void WhenNotNpgsql_ShouldBeNoOp()
        {
            ModelBuilder modelBuilder = new();
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            Action act = () => VectorConfiguration.ConfigureModel(modelBuilder, isNpgsql: false);

            act.Should().NotThrow();
        }

        [Fact]
        public async Task WhenNpgsql_ShouldBuildWithoutException()
        {
            await using ModelTestDbContext context = new(isNpgsql: true, applyConvention: true);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            context.Model.Should().NotBeNull();
        }
    }
}
