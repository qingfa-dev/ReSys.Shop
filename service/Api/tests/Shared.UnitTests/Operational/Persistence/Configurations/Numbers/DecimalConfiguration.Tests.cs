using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Configurations.Numbers;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Numbers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class DecimalConfigurationTests
{
    private sealed class TestEntity
    {
        public Int32 Id { get; set; }
        public Decimal Price { get; set; }
        public Decimal? NullablePrice { get; set; }
    }

    private sealed class ConventionTestDbContext : DbContext
    {
        public ConventionTestDbContext()
        {
        }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            DecimalConfiguration.ConfigureConvention(configurationBuilder);
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
        public async Task ShouldSetPrecisionForDecimalProperties()
        {
            await using ConventionTestDbContext context = new();
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.Price))!;

            Int32? precision = property.GetPrecision();
            Int32? scale = property.GetScale();

            precision.Should().Be(18);
            scale.Should().Be(2);
        }

        [Fact]
        public async Task ShouldSetPrecisionForNullableDecimalProperties()
        {
            await using ConventionTestDbContext context = new();
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            Microsoft.EntityFrameworkCore.Metadata.IProperty property = entityType.FindProperty(nameof(TestEntity.NullablePrice))!;

            Int32? precision = property.GetPrecision();
            Int32? scale = property.GetScale();

            precision.Should().Be(18);
            scale.Should().Be(2);
        }
    }
}
