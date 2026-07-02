using Shared.Operational.Persistence.Interceptors;
using Shared.UnitTests.Operational.Persistence.Fixtures;

namespace Shared.UnitTests.Operational.Persistence.Interceptors;

/// <summary>
/// Unit tests for <see cref="VersionableInterceptor"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class VersionableInterceptorTests
{
    /// <summary>
    /// Verifies that the Version property is incremented when an entity implementing IVersionable is modified.
    /// </summary>
    [Fact(DisplayName = "Should increment Version when entity is modified")]
    public async Task ShouldIncrementVersionOnUpdate()
    {
        var interceptor = new VersionableInterceptor();
        await using InterceptorTestDbContext context = InterceptorTestDbContextFactory.Create(interceptor);

        var entity = new TestVersionedEntity { Version = 0 };
        context.TestVersionedEntities.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Version.Should().Be(0);

        context.TestVersionedEntities.Update(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Version.Should().Be(1);
    }

    /// <summary>
    /// Verifies that the Version property is not automatically incremented during the initial addition of an entity.
    /// </summary>
    [Fact(DisplayName = "Should not increment Version when entity is added")]
    public async Task ShouldNotIncrementVersionOnAdd()
    {
        var interceptor = new VersionableInterceptor();
        await using InterceptorTestDbContext context = InterceptorTestDbContextFactory.Create(interceptor);

        var entity = new TestVersionedEntity { Version = 5 };
        context.TestVersionedEntities.Add(entity);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Version.Should().Be(5);
    }

    /// <summary>
    /// Verifies that entities not implementing IVersionable are not affected by the interceptor.
    /// </summary>
    [Fact(DisplayName = "Should not affect non-IVersionable entities")]
    public async Task ShouldNotAffectNonVersionedEntities()
    {
        var interceptor = new VersionableInterceptor();
        await using InterceptorTestDbContext context = InterceptorTestDbContextFactory.Create(interceptor);

        var entity = new TestNonVersionedEntity();
        context.TestNonVersionedEntities.Add(entity);

        Exception? exception = await Record.ExceptionAsync(async () =>
            await context.SaveChangesAsync(TestContext.Current.CancellationToken));

        exception.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the Version property remains unchanged when an entity is attached without being modified.
    /// </summary>
    [Fact(DisplayName = "Should not modify version when entity state is unchanged")]
    public async Task ShouldNotModifyWhenUnchanged()
    {
        var interceptor = new VersionableInterceptor();
        await using InterceptorTestDbContext context = InterceptorTestDbContextFactory.Create(interceptor);

        var entity = new TestVersionedEntity { Version = 10 };
        context.TestVersionedEntities.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Version.Should().Be(10);

        context.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Version.Should().Be(10);
    }

    /// <summary>
    /// Verifies that the Version property is correctly incremented through multiple successive updates.
    /// </summary>
    [Fact(DisplayName = "Should increment version multiple times on multiple updates")]
    public async Task ShouldIncrementVersionMultipleTimes()
    {
        var interceptor = new VersionableInterceptor();
        await using InterceptorTestDbContext context = InterceptorTestDbContextFactory.Create(interceptor);

        var entity = new TestVersionedEntity { Version = 0 };
        context.TestVersionedEntities.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        for (uint i = 1; i <= 3; i++)
        {
            context.TestVersionedEntities.Update(entity);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            entity.Version.Should().Be(i);
        }
    }
}
