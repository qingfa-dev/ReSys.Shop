using System.Data;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shared.Application.Models.Errors;
using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;
using Shared.Operational.Persistence.Transactions;

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

        public Task<Result> ExposedSaveChangesWithIdempotencyAsync(CancellationToken cancellationToken)
            => SaveChangesWithIdempotencyAsync(cancellationToken);
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

        public bool SupportsTransactions => false;

        public Task<IDatabaseTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default) =>
            Task.FromResult<IDatabaseTransaction>(new NoOpTransaction());
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

    private static DbUpdateException BuildDbUpdateException(String sqlState, String constraintName)
    {
        var postgresEx = new PostgresException(
            messageText: "duplicate key value violates unique constraint",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState,
            constraintName: constraintName);
        return new DbUpdateException("An error occurred while saving the entity changes.", postgresEx);
    }

    private sealed class ThrowingContext : IApplicationDbContext
    {
        public Boolean SupportsTransactions => false;

        public Task<IDatabaseTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default) =>
            Task.FromResult<IDatabaseTransaction>(new NoOpTransaction());

        public DbSet<TEntity> Set<TEntity>() where TEntity : class =>
            throw new NotImplementedException();

        public Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class ThrowingContextMockFactory
    {
        public static Mock<IApplicationDbContext> Create(DbUpdateException toThrow)
        {
            Mock<IApplicationDbContext> mock = new();
            mock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(toThrow);
            return mock;
        }
    }

    [Fact(DisplayName = "SaveChangesWithIdempotencyAsync should surface duplicate-key violations as Result.Failure")]
    public async Task SaveChangesWithIdempotencyAsync_OnDuplicateKey_ShouldReturnFailure()
    {
        var ex = BuildDbUpdateException("23505", "ix_taxa_taxonomy_slug");
        Mock<IApplicationDbContext> contextMock = ThrowingContextMockFactory.Create(ex);
        TestSeeder seeder = new(contextMock.Object);

        Result result = await seeder.ExposedSaveChangesWithIdempotencyAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNullOrEmpty();
        result.Errors[0].Code.Should().Be("Seeder.IntegrityViolation");
        result.Errors[0].Message.Should().Contain("ix_taxa_taxonomy_slug");
        result.Errors[0].Message.Should().Contain("Duplicate key");
    }

    [Fact(DisplayName = "SaveChangesWithIdempotencyAsync should surface foreign-key violations as Result.Failure")]
    public async Task SaveChangesWithIdempotencyAsync_OnForeignKeyViolation_ShouldReturnFailure()
    {
        var ex = BuildDbUpdateException("23503", "fk_classifications_taxon_taxon_id");
        Mock<IApplicationDbContext> contextMock = ThrowingContextMockFactory.Create(ex);
        TestSeeder seeder = new(contextMock.Object);

        Result result = await seeder.ExposedSaveChangesWithIdempotencyAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("Seeder.IntegrityViolation");
        result.Errors[0].Message.Should().Contain("fk_classifications_taxon_taxon_id");
        result.Errors[0].Message.Should().Contain("Foreign key");
    }

    [Fact(DisplayName = "SaveChangesWithIdempotencyAsync should return Ok on successful save")]
    public async Task SaveChangesWithIdempotencyAsync_OnSuccess_ShouldReturnOk()
    {
        Mock<IApplicationDbContext> contextMock = new();
        contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        TestSeeder seeder = new(contextMock.Object);

        Result result = await seeder.ExposedSaveChangesWithIdempotencyAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }
}
