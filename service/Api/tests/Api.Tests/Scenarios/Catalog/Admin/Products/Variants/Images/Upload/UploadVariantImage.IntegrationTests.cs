using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Images.Upload;

public sealed class UploadVariantImageIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UploadVariantImage_WithStorageDisabled_ReturnsError()
    {
        Guid variantId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]), "File", "test.jpg");
        formContent.Add(new StringContent("test-image"), "Alt");
        formContent.Add(new StringContent("0"), "Position");
        formContent.Add(new StringContent(variantId.ToString()), "VariantId");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/catalog/variant-images")
        {
            Content = formContent
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthTokenHelper.GenerateAdminToken());

        HttpResponseMessage response = await Client.SendAsync(request);
        int statusCode = (int)response.StatusCode;

        statusCode.Should().Be(404);
    }
}
