using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.Create;

public sealed class CreateOptionTypeIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateOptionType_WithValidRequest_ReturnsCreated()
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
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        OptionTypeDetailResponse? value = result.DeserializeValue<OptionTypeDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("TestColor");
        value.Presentation.Should().Be("TestColor");
        value.Position.Should().Be(1);
        value.Filterable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOptionType_WithDuplicateName_Returns409()
    {
        var request = new
        {
            name = "TestSize",
            presentation = "TestSize",
            position = 2,
            filterable = false
        };

        HttpResponseMessage firstResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", request);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOptionType_WithMissingRequiredFields_Returns422()
    {
        var request = new
        {
            presentation = "NoName",
            position = 1,
            filterable = false
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateOptionType_WithoutAuth_Returns401()
    {
        var request = new
        {
            name = "Unauthorized Option Type",
            presentation = "Unauthorized",
            position = 1,
            filterable = false
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/admin/catalog/option-types", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
