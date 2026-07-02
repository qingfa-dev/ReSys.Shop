using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Shared.Operational.Persistence.Interceptors;

namespace Shared.UnitTests.Operational.Persistence.Registration;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class PersistenceExtensionsAddPersistenceInterceptorsTests
{
    [Fact(DisplayName = "AddPersistenceInterceptors should register AuditableInterceptor")]
    public void ShouldRegisterAuditableInterceptor()
    {
        ServiceCollection services = new();
        services.AddPersistenceInterceptors();

        ServiceDescriptor[] interceptorDescriptors = services
            .Where(d => d.ServiceType == typeof(ISaveChangesInterceptor))
            .ToArray();

        interceptorDescriptors.Should().ContainSingle(d =>
            d.ImplementationType == typeof(AuditableInterceptor) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddPersistenceInterceptors should register SoftDeletableInterceptor")]
    public void ShouldRegisterSoftDeletableInterceptor()
    {
        ServiceCollection services = new();
        services.AddPersistenceInterceptors();

        ServiceDescriptor[] interceptorDescriptors = services
            .Where(d => d.ServiceType == typeof(ISaveChangesInterceptor))
            .ToArray();

        interceptorDescriptors.Should().ContainSingle(d =>
            d.ImplementationType == typeof(SoftDeletableInterceptor) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddPersistenceInterceptors should register VersionableInterceptor")]
    public void ShouldRegisterVersionableInterceptor()
    {
        ServiceCollection services = new();
        services.AddPersistenceInterceptors();

        ServiceDescriptor[] interceptorDescriptors = services
            .Where(d => d.ServiceType == typeof(ISaveChangesInterceptor))
            .ToArray();

        interceptorDescriptors.Should().ContainSingle(d =>
            d.ImplementationType == typeof(VersionableInterceptor) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddPersistenceInterceptors should register all three interceptors")]
    public void ShouldRegisterAllThreeInterceptors()
    {
        ServiceCollection services = new();
        services.AddPersistenceInterceptors();

        ServiceDescriptor[] interceptorDescriptors = services
            .Where(d => d.ServiceType == typeof(ISaveChangesInterceptor))
            .ToArray();

        interceptorDescriptors.Should().HaveCount(3);
    }
}
