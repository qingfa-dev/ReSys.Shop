using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.OptionValues.Create;

public sealed class CreateOptionValueIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
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
            "/api/catalog/option-types", request);
        ApiResponse result = await response.ReadApiResponseAsync();
        result.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? value = result.DeserializeValue<OptionTypeDetailResponse>();
        value.Should().NotBeNull();
        return value!.Id;
    }

    [Fact]
    public async Task CreateOptionValue_WithValidRequest_ReturnsCreated()
    {
        Guid optionTypeId = await CreateOptionTypeAsync();

        var request = new
        {
            name = "Red",
            presentation = "Red"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/option-types/{optionTypeId}/values", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        OptionValueListItemResponse? value = result.DeserializeValue<OptionValueListItemResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Red");
        value.Presentation.Should().Be("Red");
        value.OptionTypeId.Should().Be(optionTypeId);
    }

    [Fact]
    public async Task CreateOptionValue_WithMissingRequiredFields_Returns422()
    {
        Guid optionTypeId = await CreateOptionTypeAsync();

        var request = new
        {
            presentation = "NoName"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/option-types/{optionTypeId}/values", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateOptionValue_WithoutAuth_Returns401()
    {
        Guid optionTypeId = await CreateOptionTypeAsync();

        var request = new
        {
            name = "Unauthorized Value",
            presentation = "Unauthorized"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/catalog/option-types/{optionTypeId}/values", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
