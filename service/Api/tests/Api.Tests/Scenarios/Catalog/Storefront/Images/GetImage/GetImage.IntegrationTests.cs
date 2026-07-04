using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Catalog.Storefront.Images.GetImage;

public sealed class GetImageIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetImage_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/images/{nonexistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetImage_WithInvalidId_Returns404()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/images/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
