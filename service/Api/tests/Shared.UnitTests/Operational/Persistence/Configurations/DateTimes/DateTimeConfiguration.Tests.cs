using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Configurations.DateTimes;

namespace Shared.UnitTests.Operational.Persistence.Configurations.DateTimes;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class DateTimeConfigurationTests
{
    private sealed class TestEntity
    {
        public Int32 Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
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
            DateTimeConfiguration.ConfigureConvention(configurationBuilder, isNpgsql);
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
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.CreatedAt))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<UtcDateTimeOffsetValueConverter>();
        }

        [Fact]
        public async Task WhenNotNpgsql_ShouldApplyValueConverterForNullable()
        {
            await using ConventionTestDbContext context = new(isNpgsql: false);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.UpdatedAt))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<NullableUtcDateTimeOffsetValueConverter>();
        }

        [Fact]
        public async Task WhenNpgsql_ShouldConfigureWithoutException()
        {
            await using ConventionTestDbContext context = new(isNpgsql: true);

            Func<Task> act = async () => await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            await act.Should().NotThrowAsync();
        }
    }

    public class ConfigureProperty
    {
        [Fact]
        public void WhenNonNpgsql_ShouldApplyConverter()
        {
            ModelBuilder modelBuilder = new();
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            DateTimeConfiguration.ConfigureProperty<TestEntity>(modelBuilder, e => e.CreatedAt, isNpgsql: false);

            Microsoft.EntityFrameworkCore.Metadata.IModel model = modelBuilder.FinalizeModel();
            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.CreatedAt))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<UtcDateTimeOffsetValueConverter>();
        }

        [Fact]
        public void WhenNpgsql_ShouldNotApplyConverter()
        {
            ModelBuilder modelBuilder = new();
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            DateTimeConfiguration.ConfigureProperty<TestEntity>(modelBuilder, e => e.CreatedAt, isNpgsql: true);

            Microsoft.EntityFrameworkCore.Metadata.IModel model = modelBuilder.FinalizeModel();
            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.CreatedAt))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().BeNull();
        }
    }

    public class ConfigureNullableProperty
    {
        [Fact]
        public void WhenNonNpgsql_ShouldApplyConverter()
        {
            ModelBuilder modelBuilder = new();
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            DateTimeConfiguration.ConfigureNullableProperty<TestEntity>(modelBuilder, e => e.UpdatedAt, isNpgsql: false);

            Microsoft.EntityFrameworkCore.Metadata.IModel model = modelBuilder.FinalizeModel();
            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.UpdatedAt))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().NotBeNull();
            converter.Should().BeOfType<NullableUtcDateTimeOffsetValueConverter>();
        }

        [Fact]
        public void WhenNpgsql_ShouldNotApplyConverter()
        {
            ModelBuilder modelBuilder = new();
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            DateTimeConfiguration.ConfigureNullableProperty<TestEntity>(modelBuilder, e => e.UpdatedAt, isNpgsql: true);

            Microsoft.EntityFrameworkCore.Metadata.IModel model = modelBuilder.FinalizeModel();
            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.UpdatedAt))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().BeNull();
        }
    }
}
