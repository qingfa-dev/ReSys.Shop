using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;

namespace Shared.UnitTests.Operational.Persistence.Seeders;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class AbstractDataSeederTests
{
    private sealed class TestSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
    {
        public IApplicationDbContext ExposedContext => Context;

        public override Int32 Order => 100;

        public override Task<Result> SeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Ok());
        }
    }

    private sealed class TestEntity
    {
        public Int32 Id { get; set; }
        public String Name { get; set; } = String.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    private static TestDbContext CreateInMemoryContext(String databaseName)
    {
        DbContextOptionsBuilder<TestDbContext> optionsBuilder = new();
        optionsBuilder.UseInMemoryDatabase(databaseName);
        return new TestDbContext(optionsBuilder.Options);
    }

    [Fact(DisplayName = "Context property should be assigned from constructor")]
    public void Context_ShouldBeAssignedFromConstructor()
    {
        Mock<IApplicationDbContext> contextMock = new();
        TestSeeder seeder = new(contextMock.Object);

        seeder.ExposedContext.Should().BeSameAs(contextMock.Object);
    }

    [Fact(DisplayName = "Order should return value from concrete implementation")]
    public void Order_ShouldReturnConcreteValue()
    {
        Mock<IApplicationDbContext> contextMock = new();
        TestSeeder seeder = new(contextMock.Object);

        seeder.Order.Should().Be(100);
    }

    [Fact(DisplayName = "SeedAsync should delegate to concrete implementation")]
    public async Task SeedAsync_ShouldDelegateToConcreteImplementation()
    {
        Mock<IApplicationDbContext> contextMock = new();
        TestSeeder seeder = new(contextMock.Object);

        Result result = await seeder.SeedAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "HasDataAsync should return true when data exists")]
    public async Task HasDataAsync_ShouldReturnTrue_WhenDataExists()
    {
        TestDbContext context = CreateInMemoryContext(Guid.NewGuid().ToString());
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "test" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        TestSeeder seeder = new(context);

        Boolean result = await InvokeHasDataAsync<TestEntity>(seeder, TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "HasDataAsync should return false when no data exists")]
    public async Task HasDataAsync_ShouldReturnFalse_WhenNoDataExists()
    {
        TestDbContext context = CreateInMemoryContext(Guid.NewGuid().ToString());

        TestSeeder seeder = new(context);

        Boolean result = await InvokeHasDataAsync<TestEntity>(seeder, TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    private static async Task<Boolean> InvokeHasDataAsync<TEntity>(
        AbstractDataSeeder seeder,
        CancellationToken cancellationToken) where TEntity : class
    {
        System.Reflection.MethodInfo? method = typeof(AbstractDataSeeder)
            .GetMethod("HasDataAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Should().NotBeNull();
        Task<Boolean> task = (Task<Boolean>)method!.MakeGenericMethod(typeof(TEntity))
            .Invoke(seeder, [cancellationToken])!;
        return await task;
    }
}
