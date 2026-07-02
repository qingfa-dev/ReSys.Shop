using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

using Shared.Governance.OpenApi.Options;

namespace Shared.UnitTests.Governance.OpenApi;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OpenApi")]
public class OpenApiOptionsSetupTests
{
    [Fact(DisplayName = "ConfigureCustomOptions should return the same OpenApiOptions instance for chaining")]
    public void ConfigureCustomOptions_ShouldReturnSameInstance()
    {
        OpenApiOptions options = new OpenApiOptions();

        OpenApiOptions result = options.ConfigureCustomOptions();

        result.Should().BeSameAs(options);
    }

    [Fact(DisplayName = "ConfigureCustomOptions should throw ArgumentNullException when options is null")]
    public void ConfigureCustomOptions_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        OpenApiOptions? nullOptions = null;

        Action act = () => nullOptions!.ConfigureCustomOptions();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ConfigureCustomOptions should configure options that work with AddOpenApi pipeline")]
    public void ConfigureCustomOptions_ShouldConfigureOptionsForPipeline()
    {
        IServiceCollection services = new ServiceCollection();

        Action act = () => services.AddOpenApi(options => options.ConfigureCustomOptions());

        act.Should().NotThrow();
    }
}
