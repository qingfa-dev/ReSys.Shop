using System.Reflection;

using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Governance.Validation;

namespace Shared.UnitTests.Governance.Validation;

public class TestModel
{
    public string Name { get; set; } = string.Empty;
}

public class TestModelValidator : AbstractValidator<TestModel>
{
    public TestModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Governance")]
public class ValidationExtensionsTests
{
    [Fact(DisplayName = "AddFluentValidation should return the same WebApplicationBuilder for chaining")]
    public void AddFluentValidation_ShouldReturnSameInstance()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        WebApplicationBuilder result = builder.AddFluentValidation();

        result.Should().BeSameAs(builder);
    }

    [Fact(DisplayName = "AddFluentValidation should register validators from additional assemblies")]
    public void AddFluentValidation_ShouldRegisterAdditionalAssemblyValidators()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddFluentValidation(Assembly.GetExecutingAssembly());

        ServiceProvider provider = builder.Services.BuildServiceProvider();
        IValidator<TestModel>? validator = provider.GetService<IValidator<TestModel>>();

        validator.Should().NotBeNull();
        validator.Should().BeOfType<TestModelValidator>();
    }
}
