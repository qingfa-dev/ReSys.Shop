using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Shared.Operational.Persistence;
using Shared.Operational.Persistence.Interceptors;

namespace Shared.UnitTests.Operational.Persistence.Registration;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class PersistenceExtensionsAddNpgsqlDbContextTests
{
    private interface ITestDbContext
    {
        DbSet<TestEntity> TestEntities { get; }
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), ITestDbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    private sealed class TestEntity
    {
        public Int32 Id { get; set; }
    }

    [Fact(DisplayName = "AddNpgsqlDbContext should register interceptors when called")]
    public void AddNpgsqlDbContext_ShouldRegisterInterceptors()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.AddNpgsqlDbContext<ITestDbContext, TestDbContext>();

        ServiceDescriptor[] interceptorDescriptors = builder.Services
            .Where(d => d.ServiceType == typeof(ISaveChangesInterceptor))
            .ToArray();

        interceptorDescriptors.Should().HaveCount(3);
        interceptorDescriptors.Should().Contain(d => d.ImplementationType == typeof(AuditableInterceptor));
        interceptorDescriptors.Should().Contain(d => d.ImplementationType == typeof(SoftDeletableInterceptor));
        interceptorDescriptors.Should().Contain(d => d.ImplementationType == typeof(VersionableInterceptor));
    }

    [Fact(DisplayName = "AddNpgsqlDbContext should register TInterface mapping to TContext")]
    public void AddNpgsqlDbContext_ShouldRegisterInterfaceMapping()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.AddNpgsqlDbContext<ITestDbContext, TestDbContext>();

        builder.Services.Should().ContainSingle(d =>
            d.ServiceType == typeof(ITestDbContext) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddNpgsqlDbContext should run without throwing during registration")]
    public void AddNpgsqlDbContext_ShouldNotThrowDuringRegistration()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action act = () => builder.AddNpgsqlDbContext<ITestDbContext, TestDbContext>();

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "AddNpgsqlDbContext should accept custom connection name")]
    public void AddNpgsqlDbContext_ShouldAcceptCustomConnectionName()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action act = () => builder.AddNpgsqlDbContext<ITestDbContext, TestDbContext>("CustomConnection");

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "AddNpgsqlDbContext should accept null connection name (uses default resolution)")]
    public void AddNpgsqlDbContext_ShouldAcceptNullConnectionName()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action act = () => builder.AddNpgsqlDbContext<ITestDbContext, TestDbContext>(connectionName: null);

        act.Should().NotThrow();
    }
}
