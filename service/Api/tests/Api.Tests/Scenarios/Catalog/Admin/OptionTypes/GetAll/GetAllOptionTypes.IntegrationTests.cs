using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.GetAll;

public sealed class GetAllOptionTypesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllOptionTypes_ReturnsSeededOptionTypes()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync("/api/catalog/option-types?pageSize=100");
        PagedResult<OptionTypeListItemResponse> result = await response.ReadAsPagedResultAsync<OptionTypeListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }
}
