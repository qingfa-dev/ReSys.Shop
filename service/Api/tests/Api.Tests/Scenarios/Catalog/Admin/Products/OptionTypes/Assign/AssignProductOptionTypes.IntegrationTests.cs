using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.OptionTypes.Assign;

public sealed class AssignProductOptionTypesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task AssignProductOptionTypes_WithValidRequest_Returns200()
    {
        var createProductRequest = new
        {
            name = "Assign Option Product",
            slug = "assign-option-product"
        };
        HttpResponseMessage createProductResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createProductResult = await createProductResponse.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        var product = createProductResult.DeserializeValue<IdResponse>();
        product.Should().NotBeNull();

        var createOptionTypeRequest = new
        {
            name = "TestSize",
            presentation = "TestSize",
            position = 1,
            filterable = true
        };
        HttpResponseMessage createOptionTypeResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOptionTypeResult = await createOptionTypeResponse.ReadApiResponseAsync();
        createOptionTypeResult.IsSuccess.Should().BeTrue();
        var optionType = createOptionTypeResult.DeserializeValue<IdResponse>();
        optionType.Should().NotBeNull();

        var assignRequest = new
        {
            items = new[] { new { optionTypeId = optionType!.Id, position = 0 } }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/option-types/assign", assignRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AssignProductOptionTypes_WithNonexistentProduct_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();
        var assignRequest = new
        {
            items = new[] { new { optionTypeId = Guid.NewGuid(), position = 0 } }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{nonexistentId}/option-types/assign", assignRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
