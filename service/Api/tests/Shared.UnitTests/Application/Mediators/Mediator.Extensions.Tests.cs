using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Mediators;
using Shared.Application.Mediators.Behaviours.Exceptions;
using Shared.Application.Mediators.Behaviours.Logging;
using Shared.Application.Mediators.Behaviours.Validation;

namespace Shared.UnitTests.Application.Mediators;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Mediators")]
public class MediatorsExtensionsTests
{
    [Fact(DisplayName = "AddMediators should register MediatR services and behaviors")]
    public void AddMediators_ShouldRegisterRequiredServices()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();

        // Act
        builder.AddMediators();
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        // 1. Check MediatR
        serviceProvider.GetService<IMediator>().Should().NotBeNull();

        // 2. Check Behaviors
        var behaviors = builder.Services
            .Where(s => s.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(s => s.ImplementationType)
            .ToList();

        behaviors.Should().Contain(typeof(LoggingBehavior<,>));
        behaviors.Should().Contain(typeof(ValidationBehavior<,>));
        behaviors.Should().Contain(typeof(ExceptionMappingBehavior<,>));
    }

    [Fact(DisplayName = "AddMediators should register handlers from additional assemblies")]
    public void AddMediators_WithAdditionalAssemblies_ShouldRegisterHandlers()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var additionalAssembly = typeof(Shared.Application.Mediators.Extensions).Assembly; // Using itself as an example

        // Act
        builder.AddMediators(additionalAssembly);
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IMediator>().Should().NotBeNull();
    }

    [Fact(DisplayName = "UseMediators should resolve IMediator to warm up the service")]
    public void UseMediators_ShouldWarmUpMediator()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.AddMediators();
        var app = builder.Build();

        // Act
        var action = () => app.UseMediators();

        // Assert
        action.Should().NotThrow();
    }
}
