using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.Orders;
using Module.Profile.Domain;
using Module.Shipping.Domain.ShippingMethods;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Users;

namespace Api.Tests.Scenarios.Workflows;

public sealed class BrowseCatalogToCheckoutWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
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

    private record CartIdResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task Guest_Browse_AddToCart_Register_Checkout()
    {
        var client = Client;
        ClearAuth();

        var slug = $"wf-checkout-{Guid.NewGuid():N}";
        var createBody = new { name = "Workflow Checkout Product", slug, description = "Test" };

        HttpResponseMessage createProductResp = await client.PostAsAdminRawAsync(
            "/api/catalog/products", createBody);
        ApiResponse createProductResult = await createProductResp.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        var product = createProductResult.DeserializeValue<CreateProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage activateResp = await client.PatchAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/activate");
        activateResp.IsSuccessStatusCode.Should().BeTrue();

        Guid variantId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var hasLocation = await db.Set<StockLocation>().AnyAsync(sl => sl.Default);
            if (!hasLocation)
            {
                var locResult = StockLocationMethod.Create(
                    name: "Test Warehouse",
                    presentation: "Test Warehouse",
                    code: "TEST",
                    isDefault: true,
                    active: true,
                    propagateAllVariants: true);
                db.Set<StockLocation>().Add(locResult.Value);
                await db.SaveChangesAsync();
            }

            var location = await db.Set<StockLocation>().FirstAsync(sl => sl.Default);
            variantId = product.MasterVariantId;

            var hasStock = await db.Set<StockItem>().AnyAsync(si => si.VariantId == variantId);
            if (!hasStock)
            {
                var stockResult = StockItemMethod.Create(
                    stockLocationId: location.Id,
                    variantId: variantId,
                    countOnHand: 100,
                    backorderable: true);
                db.Set<StockItem>().Add(stockResult.Value);
                await db.SaveChangesAsync();
            }
        }

        ClearAuth();
        var browseResp = await client.GetAsync("/api/storefront/products?page=1&pageSize=10");
        browseResp.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse browseResult = await browseResp.ReadApiResponseAsync();
        browseResult.IsSuccess.Should().BeTrue();

        var addItemBody = new { variantId, quantity = 2 };
        HttpResponseMessage addItemResp = await client.PostAsJsonAsync(
            "/api/storefront/cart/items", addItemBody);
        addItemResp.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponse addItemResult = await addItemResp.ReadApiResponseAsync();
        addItemResult.IsSuccess.Should().BeTrue();
        var cart = addItemResult.DeserializeValue<CartIdResponse>();
        cart.Should().NotBeNull();
        Guid cartId = cart!.Id;

        var (email, password, userName) = TestCredentials();
        var registerBody = new { email, userName, password, firstName = "Cart", lastName = "User" };

        HttpResponseMessage registerResp = await client.PostAsJsonAsync(
            "/api/store/identity/auth/register", registerBody);
        registerResp.IsSuccessStatusCode.Should().BeTrue();

        var loginBody = new { credential = email, password };
        HttpResponseMessage loginResp = await client.PostAsJsonAsync(
            "/api/store/identity/auth/login/password", loginBody);
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
            db.Set<Module.Profile.Domain.UserProfile>().Add(profileResult.Value);
            await db.SaveChangesAsync();
        }

        var associateBody = new { guestOrderId = cartId };
        HttpResponseMessage associateResp = await client.PostAsJsonAsync(
            "/api/storefront/cart/associate", associateBody);
        associateResp.IsSuccessStatusCode.Should().BeTrue();

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
            "/api/store/profiles/addresses", addressBody);
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
        HttpResponseMessage updateCartResp = await client.PutAsJsonAsync(
            "/api/storefront/cart", updateCartBody);
        updateCartResp.IsSuccessStatusCode.Should().BeTrue();

        Guid shippingMethodId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var shippingMethod = await db.Set<ShippingMethod>()
                .FirstAsync(sm => sm.Code == "standard");
            shippingMethodId = shippingMethod.Id;
        }

        var selectShipBody = new { shippingMethodId };
        HttpResponseMessage selectShipResp = await client.PostAsJsonAsync(
            "/api/storefront/cart/shipping-rate", selectShipBody);
        selectShipResp.IsSuccessStatusCode.Should().BeTrue();

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
            cartEntity!.CheckoutState = CheckoutState.Confirm;

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
