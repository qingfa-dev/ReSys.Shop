using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Shared.Operational.Persistence;
using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Interceptors;

namespace Shared.UnitTests.Operational.Persistence.Registration;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class PersistenceExtensionsAddPersistenceTests
{
    [Fact(DisplayName = "AddPersistence should register interceptors when called")]
    public void AddPersistence_ShouldRegisterInterceptors()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.AddPersistence();

        ServiceDescriptor[] interceptorDescriptors = builder.Services
            .Where(d => d.ServiceType == typeof(ISaveChangesInterceptor))
            .ToArray();

        interceptorDescriptors.Should().HaveCount(3);
        interceptorDescriptors.Should().Contain(d => d.ImplementationType == typeof(AuditableInterceptor));
        interceptorDescriptors.Should().Contain(d => d.ImplementationType == typeof(SoftDeletableInterceptor));
        interceptorDescriptors.Should().Contain(d => d.ImplementationType == typeof(VersionableInterceptor));
    }

    [Fact(DisplayName = "AddPersistence should register IApplicationDbContext mapping")]
    public void AddPersistence_ShouldRegisterIApplicationDbContext()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.AddPersistence();

        builder.Services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IApplicationDbContext) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddPersistence should run without throwing during registration")]
    public void AddPersistence_ShouldNotThrowDuringRegistration()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action act = () => builder.AddPersistence();

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "AddPersistence should accept additional assemblies")]
    public void AddPersistence_ShouldAcceptAdditionalAssemblies()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action act = () => builder.AddPersistence(typeof(String).Assembly);

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "AddPersistence should accept multiple additional assemblies")]
    public void AddPersistence_ShouldAcceptMultipleAdditionalAssemblies()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action act = () => builder.AddPersistence(typeof(String).Assembly, typeof(Uri).Assembly);

        act.Should().NotThrow();
    }
}
