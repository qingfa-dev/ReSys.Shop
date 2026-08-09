using System.Net;
using System.Text.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Storefront.Products.List;

public sealed class ListProductsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task ListProducts_WithActiveProducts_ReturnsItems()
    {
        var createRequest = new
        {
            name = "Listable Product",
            slug = "listable-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        using var activateRequest = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/admin/catalog/products/{productId}/activate");
        activateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListProducts_WithTextSearch_ReturnsMatchingResults()
    {
        var createRequest = new
        {
            name = "Searchable Product",
            slug = "searchable-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        using var activateRequest = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/admin/catalog/products/{productId}/activate");
        activateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products?search=Searchable");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListProducts_WithPagination_RespectsPageSize()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();
        result.IsSuccess.Should().BeTrue();
    }

    private async Task<Guid> CreateOptionTypeAsync()
    {
        var request = new
        {
            name = "TestColor",
            presentation = "TestColor",
            position = 1,
            filterable = true
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", request);
        ApiResponse result = await response.ReadApiResponseAsync();
        result.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? value = result.DeserializeValue<OptionTypeDetailResponse>();
        value.Should().NotBeNull();
        return value!.Id;
    }

    [Fact]
    public async Task ListProducts_WithOptionValueIdParam_ReturnsOk()
    {
        Guid optionTypeId = await CreateOptionTypeAsync();
        var request = new
        {
            name = "Red",
            presentation = "Red",
            optionTypeId = optionTypeId
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-values", request);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        OptionValueListItemResponse? value = createResult.DeserializeValue<OptionValueListItemResponse>();
        value.Should().NotBeNull();
        Guid optionValueId = value!.Id;

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products?optionValueId={optionValueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListProducts_WithPriceRange_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products?minPrice=1&maxPrice=1000");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListProducts_WithUnwhitelistedRawFilter_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products?filter=NotARealField=oops");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListProducts_WithTypedParamAndRawFilter_ReturnsOk()
    {
        Guid optionTypeId = await CreateOptionTypeAsync();
        var request = new
        {
            name = "Red",
            presentation = "Red",
            optionTypeId = optionTypeId
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-values", request);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        OptionValueListItemResponse? value = createResult.DeserializeValue<OptionValueListItemResponse>();
        value.Should().NotBeNull();
        Guid optionValueId = value!.Id;

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products?optionValueId={optionValueId}&filter=Variants.OptionValueVariants.OptionValue.OptionType.Name=Color");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
