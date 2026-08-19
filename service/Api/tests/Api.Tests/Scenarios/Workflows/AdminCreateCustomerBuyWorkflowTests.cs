using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Ordering.Domain.Orders;
using Module.Customer.Domain;
using Module.Shipping.Domain.ShippingMethods;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Users;

namespace Api.Tests.Scenarios.Workflows;

public sealed class AdminCreateCustomerBuyWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    private record CreateProductResponse
    {
        public Guid Id { get; init; }
        public Guid MasterVariantId { get; init; }
    }

    private record AddressIdResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task Admin_Creates_Product_Customer_Browses_And_Buys()
    {
        var client = Client;
        ClearAuth();

        var slug = $"wf-admin-{Guid.NewGuid():N}";
        var createBody = new { name = "Admin Workflow Product", slug, description = "Admin created" };

        HttpResponseMessage createResp = await client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createBody);
        ApiResponse createResult = await createResp.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<CreateProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage activateResp = await client.PatchAsAdminRawAsync(
            $"/api/admin/catalog/products/{product!.Id}/activate");
        activateResp.IsSuccessStatusCode.Should().BeTrue();

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            if (!await db.Set<StockLocation>().AnyAsync(sl => sl.Default))
            {
                var locResult = StockLocationMethod.Create(
                    name: "Warehouse",
                    presentation: "Warehouse",
                    code: "WH",
                    isDefault: true,
                    active: true,
                    propagateAllVariants: true);
                db.Set<StockLocation>().Add(locResult.Value);
                await db.SaveChangesAsync();
            }
            var location = await db.Set<StockLocation>().FirstAsync(sl => sl.Default);
            if (!await db.Set<StockItem>().AnyAsync(si => si.VariantId == product.MasterVariantId))
            {
                var stockResult = StockItemMethod.Create(
                    stockLocationId: location.Id,
                    variantId: product.MasterVariantId,
                    countOnHand: 50,
                    backorderable: true);
                db.Set<StockItem>().Add(stockResult.Value);
                await db.SaveChangesAsync();
            }
        }

        ClearAuth();
        var browseResp = await client.GetAsync("/api/storefront/products?page=1&pageSize=20");
        browseResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var (email, password, userName) = TestCredentials();
        var registerBody = new { email, userName, password, firstName = "Buyer", lastName = "Smith" };

        HttpResponseMessage registerResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/register", registerBody);
        registerResp.IsSuccessStatusCode.Should().BeTrue();

        var loginBody = new { credential = email, password };
        HttpResponseMessage loginResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", loginBody);
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse loginResult = await loginResp.ReadApiResponseAsync();
        loginResult.IsSuccess.Should().BeTrue();

        string accessToken = IdentityTestHelper.GetAccessToken(loginResult);
        accessToken.Should().NotBeNullOrEmpty();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            var profileResult = UserProfileMethod.Create("Test", "User", email, userId: user!.Id);
            profileResult.IsSuccess.Should().BeTrue();
            db.Set<UserProfile>().Add(profileResult.Value);
            await db.SaveChangesAsync();
        }

        var addItemBody = new { variantId = product.MasterVariantId, quantity = 1 };
        HttpResponseMessage addResp = await client.PostAsJsonAsync(
            "/api/storefront/cart/items", addItemBody);
        addResp.IsSuccessStatusCode.Should().BeTrue();

        var addressBody = new
        {
            addressType = "Shipping",
            firstName = "Test",
            lastName = "User",
            address1 = "123 Test St",
            city = "TestCity",
            stateProvince = "California",
            countryName = "United States",
            zipCode = "90210",
            phone = "555-0100"
        };

        HttpResponseMessage addressResp = await client.PostAsJsonAsync(
            "/api/storefront/profiles/addresses", addressBody);
        addressResp.IsSuccessStatusCode.Should().BeTrue();
        ApiResponse addressResult = await addressResp.ReadApiResponseAsync();
        var address = addressResult.DeserializeValue<AddressIdResponse>();
        address.Should().NotBeNull();
        Guid shipAddressId = address!.Id;

        var updateCartBody = new
        {
            email,
            billAddressId = shipAddressId,
            shipAddressId = shipAddressId
        };
        HttpResponseMessage updateCartResp = await client.PatchAsJsonAsync(
            "/api/storefront/cart", updateCartBody);
        updateCartResp.IsSuccessStatusCode.Should().BeTrue();

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();

            var cartEntity = await db.Set<Order>()
                .Include(o => o.LineItems)
                .Include(o => o.Adjustments)
                .FirstOrDefaultAsync(o => o.UserId == user!.Id && o.Status == OrderStatus.Draft);
            cartEntity.Should().NotBeNull();

            var existingSm = await db.Set<ShippingMethod>()
                .FirstOrDefaultAsync(sm => sm.Code == "standard");
            if (existingSm is not null)
            {
                cartEntity!.ShippingMethodId = existingSm.Id;
            }
            else
            {
                var smResult = ShippingMethodExtensions.Create(
                    name: "Standard Shipping",
                    calculatorType: "flat_rate",
                    code: "standard");
                db.Set<ShippingMethod>().Add(smResult.Value);
                await db.SaveChangesAsync();
                cartEntity!.ShippingMethodId = smResult.Value.Id;
            }

            cartEntity.CheckoutState = CheckoutState.Confirm;

            var recalcResult = cartEntity.RecalculateTotals();
            recalcResult.IsSuccess.Should().BeTrue();

            await db.SaveChangesAsync();
        }

        var checkoutBody = new { paymentIntentId = (string?)null };
        HttpResponseMessage checkoutResp = await client.PostAsJsonAsync(
            "/api/storefront/cart/checkout", checkoutBody);
        checkoutResp.IsSuccessStatusCode.Should().BeTrue();
    }
}
