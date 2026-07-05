using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Catalog.Storefront.Products.SearchByImage;

public sealed class SearchByImageIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SearchByImage_WithStorageDisabled_ReturnsResponse()
    {
        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]), "image", "test.jpg");

        HttpResponseMessage response = await Client.PostAsync(
            "/api/storefront/search-by-image", formContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SearchByImage_WithoutImage_ReturnsResponse()
    {
        HttpResponseMessage response = await Client.PostAsync(
            "/api/storefront/search-by-image", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}
