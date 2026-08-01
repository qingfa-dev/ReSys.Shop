using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Images.Delete;

public sealed class DeleteVariantImageIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteVariantImage_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.DeleteAsAdminRawAsync(
            $"/api/catalog/variant-images/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
