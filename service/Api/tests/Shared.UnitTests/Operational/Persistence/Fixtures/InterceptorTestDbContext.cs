using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shared.Application.Domain.Models;

namespace Shared.UnitTests.Operational.Persistence.Fixtures;

public class InterceptorTestDbContext(DbContextOptions<InterceptorTestDbContext> options) : DbContext(options)
{
    public DbSet<TestAuditableEntity> TestAuditableEntities => Set<TestAuditableEntity>();
    public DbSet<TestVersionedEntity> TestVersionedEntities => Set<TestVersionedEntity>();
    public DbSet<TestSoftDeletedEntity> TestSoftDeletedEntities => Set<TestSoftDeletedEntity>();
    public DbSet<Entity> TestAggregateRoots => Set<Entity>();
    public DbSet<TestNonAuditableEntity> TestNonAuditableEntities => Set<TestNonAuditableEntity>();
    public DbSet<TestNonVersionedEntity> TestNonVersionedEntities => Set<TestNonVersionedEntity>();
    public DbSet<TestNonSoftDeletedEntity> TestNonSoftDeletedEntities => Set<TestNonSoftDeletedEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Key is configured on the root Entity type below.
        // Derived types inherit the key.

        modelBuilder.Entity<TestSoftDeletedEntity>(b =>
        {
            b.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<Entity>(b => b.HasKey(e => e.Id));
    }
}

public static class InterceptorTestDbContextFactory
{
    public static InterceptorTestDbContext Create(params IInterceptor[] interceptors)
    {
        DbContextOptionsBuilder<InterceptorTestDbContext> optionsBuilder = new DbContextOptionsBuilder<InterceptorTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        optionsBuilder.AddInterceptors(interceptors);

        return new InterceptorTestDbContext(optionsBuilder.Options);
    }
}
