using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.OptionValues.GetAll;

public sealed class GetAllOptionValuesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllOptionValues_ReturnsOptionValues()
    {
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var values = await dbContext.Set<OptionValue>().ToListAsync();
            dbContext.Set<OptionValue>().RemoveRange(values);
            var types = await dbContext.Set<OptionType>().ToListAsync();
            dbContext.Set<OptionType>().RemoveRange(types);
            await dbContext.SaveChangesAsync();
        }

        var createOptionTypeRequest = new
        {
            name = "TestSize",
            presentation = "TestSize",
            position = 1,
            filterable = true
        };

        HttpResponseMessage createOtResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOtResult = await createOtResponse.ReadApiResponseAsync();
        createOtResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? optionType = createOtResult.DeserializeValue<OptionTypeDetailResponse>();
        optionType.Should().NotBeNull();
        Guid optionTypeId = optionType!.Id;

        var createValueRequest = new
        {
            name = "Small",
            presentation = "Small",
            optionTypeId = optionTypeId
        };

        HttpResponseMessage createValResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-values", createValueRequest);
        createValResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/admin/catalog/option-values?pageSize=100");
        PagedResult<OptionValueListItemResponse> result = await response.ReadAsPagedResultAsync<OptionValueListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(v => v.Name == "Small");
    }
}
