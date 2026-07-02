using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Configurations.Enums;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Enums;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class EnumConfigurationTests
{
    private sealed class TestEntity
    {
        public Int32 Id { get; set; }
        public TestColor Color { get; set; }
        public TestColor? NullableColor { get; set; }
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
            EnumConfiguration.ConfigureConvention(configurationBuilder, isNpgsql);
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
        public async Task WhenNotNpgsql_ShouldBuildModelWithoutException()
        {
            await using ConventionTestDbContext context = new(isNpgsql: false);

            Func<Task> act = async () => await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task WhenNpgsql_ShouldNotApplyConversion()
        {
            await using ConventionTestDbContext context = new(isNpgsql: true);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.Color))!;
            Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter = property.GetValueConverter();

            converter.Should().BeNull();
        }
    }

    public class ConfigureModel
    {
        [Fact]
        public void WhenNotNpgsql_ShouldBeNoOp()
        {
            ModelBuilder modelBuilder = new();

            Action act = () => EnumConfiguration.ConfigureModel(modelBuilder, isNpgsql: false);

            act.Should().NotThrow();
        }
    }
}
