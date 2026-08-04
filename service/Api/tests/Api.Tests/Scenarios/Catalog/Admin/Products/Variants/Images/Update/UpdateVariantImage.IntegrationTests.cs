using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Images.Update;

public sealed class UpdateVariantImageIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateVariantImage_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();
        var request = new { alt = "Updated Alt", position = 1 };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put,
            $"/api/catalog/variant-images/{nonexistentId}")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());

        HttpResponseMessage response = await Client.SendAsync(httpRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
