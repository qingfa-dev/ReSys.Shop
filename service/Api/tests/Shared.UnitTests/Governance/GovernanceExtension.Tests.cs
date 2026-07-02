using System.Reflection;

using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Governance;

namespace Shared.UnitTests.Governance;

public class TestGovernanceModel
{
    public string Email { get; set; } = string.Empty;
}

public class TestGovernanceModelValidator : AbstractValidator<TestGovernanceModel>
{
    public TestGovernanceModelValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Governance")]
public class GovernanceExtensionTests
{
    [Fact(DisplayName = "AddGovernance should return the same WebApplicationBuilder for chaining")]
    public void AddGovernance_ShouldReturnSameInstance()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        WebApplicationBuilder result = builder.AddGovernance();

        result.Should().BeSameAs(builder);
    }

    [Fact(DisplayName = "AddGovernance should register FluentValidation validators from additional assemblies")]
    public void AddGovernance_ShouldRegisterFluentValidation()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddGovernance(Assembly.GetExecutingAssembly());

        ServiceProvider provider = builder.Services.BuildServiceProvider();
        IValidator<TestGovernanceModel>? validator = provider.GetService<IValidator<TestGovernanceModel>>();

        validator.Should().NotBeNull();
        validator.Should().BeOfType<TestGovernanceModelValidator>();
    }

    [Fact(DisplayName = "AddGovernance should register OpenAPI documentation services")]
    public void AddGovernance_ShouldRegisterOpenApiServices()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddGovernance();

        Boolean openApiServiceFound = builder.Services.Any(
            s => s.ServiceType.FullName?.Contains("OpenApi") == true);

        openApiServiceFound.Should().BeTrue();
    }

    [Fact(DisplayName = "UseGovernance should return the same WebApplication for chaining")]
    public void UseGovernance_ShouldReturnSameInstance()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        WebApplication result = app.UseGovernance();

        result.Should().BeSameAs(app);
    }

    [Fact(DisplayName = "UseGovernance should throw ArgumentNullException when app is null")]
    public void UseGovernance_ShouldThrowArgumentNullException_WhenAppIsNull()
    {
        WebApplication? nullApp = null;

        Action act = () => nullApp!.UseGovernance();

        act.Should().Throw<ArgumentNullException>();
    }
}
