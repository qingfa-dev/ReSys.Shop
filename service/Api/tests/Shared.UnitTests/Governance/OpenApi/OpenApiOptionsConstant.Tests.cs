using Shared.Governance.OpenApi.Options;

namespace Shared.UnitTests.Governance.OpenApi;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OpenApi")]
public class OpenApiOptionsConstantTests
{
    [Fact(DisplayName = "Title should be 'ReSys Shop API'")]
    public void Title_ShouldBeReSysShopApi()
    {
        OpenApiOptionsConstant.Info.Title.Should().Be("ReSys Shop API");
    }

    [Fact(DisplayName = "Version should be 'v1'")]
    public void Version_ShouldBeV1()
    {
        OpenApiOptionsConstant.Info.Version.Should().Be("v1");
    }

    [Fact(DisplayName = "Description should be the expected string")]
    public void Description_ShouldBeExpectedString()
    {
        OpenApiOptionsConstant.Info.Description.Should().Be("Production-grade e-commerce API infrastructure.");
    }

    [Fact(DisplayName = "Endpoint should be '/api/scalar'")]
    public void Endpoint_ShouldBeApiScalar()
    {
        OpenApiOptionsConstant.Info.Endpoint.Should().Be("/api/scalar");
    }
}
