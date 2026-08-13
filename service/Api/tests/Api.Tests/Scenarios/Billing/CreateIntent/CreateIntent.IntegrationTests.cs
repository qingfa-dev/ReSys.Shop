using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Module.Billing.Domain.PaymentMethods;
using Module.Catalog.Domain.Variants;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockLocations;
using Module.Ordering.Domain.Orders;

using Shared.Operational.Persistence.Configurations.Dictionaries;
using Shared.Operational.Persistence.Data;
using Shared.Operational.Security.Encryption;

using BogusGateway = Module.Billing.Services.Provider.Bogus.BogusGateway;
using BogusSetting = Module.Billing.Services.Provider.Bogus.BogusSetting;
using GatewayConstants = Module.Billing.Services.Provider.GatewayConstants;
using GatewayRegistry = Module.Billing.Services.Provider.GatewayRegistry;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;

namespace Api.Tests.Scenarios.Payment.CreateIntent;

public sealed class CreateIntentIntegrationTests(ApiFixture fixture) : PaymentIntegrationTestBase(fixture)
{
    private record CreateProductResponse
    {
        public Guid Id { get; init; }
        public Guid MasterVariantId { get; init; }
    }

    private record CartIdResponse
    {
        public Guid Id { get; init; }
    }

    private sealed record CreateIntentResponse
    {
        public string State { get; init; } = string.Empty;
        public string? CheckoutUrl { get; init; }
    }

    [Fact]
    public async Task CreateIntent_WithoutAuth_Returns401()
    {
        var request = new { orderId = Guid.NewGuid() };
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/cart/payment/intent", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "COD intent returns Pending without a gateway")]
    public async Task CreateIntent_Cod_ReturnsPending()
    {
        Guid cartId = await CreateCartAtDeliveryAsync();
        Guid codMethodId = await EnsurePaymentMethodAsync(
            "Cash on Delivery", "cash_on_delivery", GatewayConstants.Providers.CashOnDelivery);

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/storefront/cart/payment/intent",
            new { orderId = cartId, paymentMethodId = codMethodId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse result = await response.ReadApiResponseAsync();
        result.IsSuccess.Should().BeTrue();
        var intent = result.DeserializeValue<CreateIntentResponse>();
        intent.Should().NotBeNull();
        intent!.State.Should().Be("Pending");
        intent.CheckoutUrl.Should().BeNull();
    }

    [Fact(DisplayName = "Card intent returns a checkout URL")]
    public async Task CreateIntent_Card_ReturnsCheckoutUrl()
    {
        // Register the offline Bogus gateway for the "stripe" provider key so the
        // card intent resolves to a fake Checkout Session instead of the real Stripe API.
        // NOTE: GatewayRegistry is a DI singleton — the registration persists for the
        // whole test suite; no test currently relies on a real Stripe gateway.
        var scope = Fixture.Factory.Services.CreateScope();
        try
        {
            var registry = (GatewayRegistry)scope.ServiceProvider.GetRequiredService<IGatewayRegistry>();
            registry.Register(
                GatewayConstants.Providers.Stripe,
                () => new BogusGateway(Options.Create(new BogusSetting())));
        }
        finally
        {
            scope.Dispose();
        }

        Guid cartId = await CreateCartAtDeliveryAsync();
        Guid stripeMethodId = await EnsurePaymentMethodAsync(
            "Credit Card", "credit_card", GatewayConstants.Providers.Stripe);

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/storefront/cart/payment/intent",
            new
            {
                orderId = cartId,
                paymentMethodId = stripeMethodId,
                returnUrl = "https://store.test/checkout/return",
                cancelUrl = "https://store.test/checkout",
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse result = await response.ReadApiResponseAsync();
        result.IsSuccess.Should().BeTrue();
        var intent = result.DeserializeValue<CreateIntentResponse>();
        intent.Should().NotBeNull();
        intent!.CheckoutUrl.Should().NotBeNull();
    }

    private async Task<Guid> CreateCartAtDeliveryAsync()
    {
        var slug = $"intent-test-{Guid.NewGuid():N}";
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products",
            new { name = "Intent Test Product", slug, description = "Test product for payment intent" });
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

            var variant = await dbContext.Set<Variant>()
                .FirstAsync(v => v.Id == created.MasterVariantId);
            variant.Price = 100m;

            var hasDefaultLocation = await dbContext.Set<StockLocation>().AnyAsync(sl => sl.Default);
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

            var location = await dbContext.Set<StockLocation>().FirstAsync(sl => sl.Default);
            var hasStock = await dbContext.Set<StockItem>()
                .AnyAsync(si => si.VariantId == created.MasterVariantId);
            if (!hasStock)
            {
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
            "/api/storefront/cart/items",
            new { variantId = created.MasterVariantId, quantity = 1 });
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponse addResult = await addResponse.ReadApiResponseAsync();
        addResult.IsSuccess.Should().BeTrue();
        var cart = addResult.DeserializeValue<CartIdResponse>();
        cart.Should().NotBeNull();

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var order = await dbContext.Set<Order>().FirstAsync(o => o.Id == cart!.Id);
            order.CheckoutState = CheckoutState.Delivery;
            await dbContext.SaveChangesAsync();
        }

        return cart!.Id;
    }

    private async Task<Guid> EnsurePaymentMethodAsync(string name, string? code, string providerKey)
    {
        // The encrypted Settings dictionary on PaymentMethod needs the converter
        // wired to the DI container; ApiFactory strips the startup hosted service
        // that normally does this, so configure it from the test factory instead.
        EncryptedDictionaryConverter.Configure(sp => sp.GetRequiredService<IEncryptionService>());
        EncryptedDictionaryConverter.ConfigureServiceProvider(Fixture.Factory.Services);

        using var scope = Fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var existing = await dbContext.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.ProviderKey == providerKey && !pm.IsDeleted);
        if (existing is not null)
            return existing.Id;

        var createResult = PaymentMethodMethod.Create(name, code, providerKey);
        createResult.IsSuccess.Should().BeTrue();
        dbContext.Set<PaymentMethod>().Add(createResult.Value);
        await dbContext.SaveChangesAsync();
        return createResult.Value.Id;
    }
}
