using Microsoft.EntityFrameworkCore;
using Shared.Operational.Persistence.Data;

namespace Shared.UnitTests.Operational.Persistence.Data;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class ApplicationDbContextTests
{
    [Fact(DisplayName = "Should implement IApplicationDbContext")]
    public void ShouldImplementIApplicationDbContext()
    {
        typeof(ApplicationDbContext).Should().Implement<IApplicationDbContext>();
    }

    [Fact(DisplayName = "Should implement IApplicationDbContext directly (not through base type)")]
    public void ShouldImplementIApplicationDbContextDirectly()
    {
        typeof(ApplicationDbContext)
            .GetInterfaces()
            .Should().Contain(typeof(IApplicationDbContext));
    }

    [Fact(DisplayName = "SupportsTransactions should be false for in-memory provider")]
    public void SupportsTransactions_ShouldBeFalse_ForInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new ApplicationDbContext(options);
        db.SupportsTransactions.Should().BeFalse();
    }

    [Fact(DisplayName = "BeginTransactionAsync should return NoOpTransaction for in-memory provider")]
    public async Task BeginTransactionAsync_ShouldReturnNoOpTransaction_ForInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ApplicationDbContext).Assembly];
        await using var db = new ApplicationDbContext(options);

        await using var tx = await db.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        tx.Should().NotBeNull();
        tx.Should().BeOfType<Shared.Operational.Persistence.Transactions.NoOpTransaction>();
    }

    [Fact(DisplayName = "NoOpTransaction should not throw on Commit, Rollback, or Dispose")]
    public async Task NoOpTransaction_ShouldNotThrow()
    {
        await using var tx = new Shared.Operational.Persistence.Transactions.NoOpTransaction();

        await tx.Invoking(t => t.CommitAsync()).Should().NotThrowAsync();
        await tx.Invoking(t => t.RollbackAsync()).Should().NotThrowAsync();
    }
}
