using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.GetAll;

public sealed class GetAllProductsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllProducts_ReturnsSeededProducts()
    {
        var createRequest = new { name = "Test Get Product", slug = "test-get-product" };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync("/api/admin/catalog/products", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync("/api/admin/catalog/products?pageSize=100");
        PagedResult<ProductListItemResponse> result = await response.ReadAsPagedResultAsync<ProductListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllProducts_WithPagination_ReturnsCorrectPage()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync("/api/admin/catalog/products?pageNumber=1&pageSize=5");
        PagedResult<ProductListItemResponse> result = await response.ReadAsPagedResultAsync<ProductListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.Items.Should().NotBeNull();
    }
}
