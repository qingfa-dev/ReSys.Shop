using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class AddItemIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    public record CreateProductResponse
    {
        public Guid Id { get; init; }
        public Guid MasterVariantId { get; init; }
    }

    [Fact]
    public async Task AddItem_WithoutAuth_Returns201()
    {
        var slug = $"additem-test-{Guid.NewGuid():N}";
        var createRequest = new
        {
            name = "AddItem Test Product",
            slug,
            description = "Test product for cart add item"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateProductResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage activateResponse = await Client.PatchAsAdminRawAsync(
            $"/api/admin/catalog/products/{created!.Id}/activate");
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var hasDefaultLocation = await dbContext.Set<StockLocation>()
                .AnyAsync(sl => sl.Default);
            if (!hasDefaultLocation)
            {
                var locationResult = StockLocationMethod.Create(
                    name: "Test Warehouse",
                    presentation: "Test Warehouse",
                    code: "TEST",
                    isDefault: true,
                    active: true,
                    propagateAllVariants: true);
                dbContext.Set<StockLocation>().Add(locationResult.Value);
                await dbContext.SaveChangesAsync();
            }

            var hasStock = await dbContext.Set<StockItem>()
                .AnyAsync(si => si.VariantId == created.MasterVariantId);
            if (!hasStock)
            {
                var location = await dbContext.Set<StockLocation>()
                    .FirstAsync(sl => sl.Default);
                var stockResult = StockItemMethod.Create(
                    stockLocationId: location.Id,
                    variantId: created.MasterVariantId,
                    countOnHand: 100,
                    backorderable: true);
                dbContext.Set<StockItem>().Add(stockResult.Value);
                await dbContext.SaveChangesAsync();
            }
        }

        var request = new { variantId = created.MasterVariantId, quantity = 1 };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/storefront/cart/items", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);

        string? setCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        setCookie.Should().NotBeNull();
        setCookie.Should().Contain("Guest=");
    }
}
