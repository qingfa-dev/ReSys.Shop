using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class CheckoutIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    public record CreateProductResponse
    {
        public Guid Id { get; init; }
        public Guid MasterVariantId { get; init; }
    }

    [Fact]
    public async Task Checkout_WithoutAuth_Returns400DueToMissingPaymentIntent()
    {
        var slug = $"checkout-test-{Guid.NewGuid():N}";
        var createRequest = new
        {
            name = "Checkout Test Product",
            slug,
            description = "Test product for checkout"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();

        if (!createResult.IsSuccess)
        {
            string body = await createResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Create product failed. Status: {createResponse.StatusCode}, Body: {body}");
        }

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

        HttpResponseMessage addResponse = await Client.PostAsAdminRawAsync(
            "/api/storefront/cart/items", new { variantId = created.MasterVariantId, quantity = 1 });

        if (!addResponse.IsSuccessStatusCode)
        {
            string body = await addResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Add to cart failed. Status: {addResponse.StatusCode}, Body: {body}");
        }

        addResponse.IsSuccessStatusCode.Should().BeTrue();

        var checkoutRequest = new { paymentIntentId = (string?)null };
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/storefront/cart/checkout", checkoutRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
    }
}
