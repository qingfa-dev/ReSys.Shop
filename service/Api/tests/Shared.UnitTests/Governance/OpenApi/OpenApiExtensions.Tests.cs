using Microsoft.AspNetCore.Builder;

using Shared.Governance.OpenApi;

namespace Shared.UnitTests.Governance.OpenApi;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OpenApi")]
public class OpenApiExtensionsTests
{
    [Fact(DisplayName = "AddOpenApiDocumentation should return the same WebApplicationBuilder for chaining")]
    public void AddOpenApiDocumentation_ShouldReturnSameInstance()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        WebApplicationBuilder result = builder.AddOpenApiDocumentation();

        result.Should().BeSameAs(builder);
    }

    [Fact(DisplayName = "UseOpenApiDocumentation should return the same WebApplication for chaining")]
    public void UseOpenApiDocumentation_ShouldReturnSameInstance()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        WebApplication result = app.UseOpenApiDocumentation();

        result.Should().BeSameAs(app);
    }
}
