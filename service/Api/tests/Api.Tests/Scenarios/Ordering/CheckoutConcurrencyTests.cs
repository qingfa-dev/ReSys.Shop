using Api.Tests.Infrastructure;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;

using Shared.Application.Models.Errors;
using Shared.Application.Models.Results;
using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Ordering;

[Trait("Category", "Integration")]
[Trait("Module", "Ordering")]
public sealed class CheckoutConcurrencyTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AC-ORD-010: 1 unit of stock, 2 concurrent checkouts -> exactly 1 succeeds")]
    public async Task TwoConcurrentCheckouts_OnlyOneSucceeds()
    {
        // Arrange: seed 1 unit of stock and 2 draft carts (one per user)
        var (variantId, userA, userB, locationId) = await SeedFixturesAsync();

        Guid cartAId = await SeedDraftCartAsync(userA, variantId);
        Guid cartBId = await SeedDraftCartAsync(userB, variantId);

        try
        {
            // Act: fire 2 checkouts concurrently. Each runs in its own
            // service scope so the underlying DbContext (and Serializable
            // transaction) is isolated. The TestCurrentUser stub is keyed
            // on AsyncLocal<Guid?> so the per-task user id flows with the
            // async context.
            Task<Result<CreateOrderFromCart.Response>> taskA =
                SendAsUserAsync(userA, new CreateOrderFromCart.Command(new()));

            Task<Result<CreateOrderFromCart.Response>> taskB =
                SendAsUserAsync(userB, new CreateOrderFromCart.Command(new()));

            Result<CreateOrderFromCart.Response>[] results = await Task.WhenAll(taskA, taskB);

            // Assert: exactly 1 success and 1 failure. The failure can be
            // StockItem.InsufficientStock (committed state observed) or a
            // serialization-conflict surfaced as DbUpdateException — both
            // are acceptable for the AC and the helper converts the
            // exception into a failure Result.
            int successes = results.Count(r => r.IsSuccess);
            int failures = results.Count(r => r.IsFailure);
            successes.Should().Be(1, "exactly one of the concurrent checkouts must succeed");
            failures.Should().Be(1, "exactly one of the concurrent checkouts must fail");

            // Stock on hand must be zero (1 - 1 = 0)
            using IServiceScope verifyScope = Fixture.Factory.Services.CreateScope();
            IApplicationDbContext verifyDb = verifyScope.ServiceProvider
                .GetRequiredService<IApplicationDbContext>();

            int stockAfter = await verifyDb.Set<StockItem>()
                .Where(si => si.VariantId == variantId)
                .SumAsync(si => si.CountOnHand, TestContext.Current.CancellationToken);

            stockAfter.Should().Be(0, "the successful checkout must have deducted the only unit");

            // Exactly one Order-originated StockMovement must exist for the
            // two carts. The failing checkout must not have leaked any
            // partial-mutation side effects (no second movement, no half-
            // deducted stock).
            int movements = await verifyDb
                .Set<Module.Inventory.Domain.StockLocations.StockItems.StockMovements.StockMovement>()
                .Where(m => m.OriginatorType == "Order"
                            && (m.OriginatorId == cartAId || m.OriginatorId == cartBId))
                .CountAsync(TestContext.Current.CancellationToken);

            movements.Should().Be(1, "only the winning checkout records a stock movement");
        }
        finally
        {
            ApiFactory.TestCurrentUser.Reset();
        }
    }

    private async Task<(Guid VariantId, Guid UserA, Guid UserB, Guid LocationId)> SeedFixturesAsync()
    {
        using IServiceScope scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        Guid variantId = Guid.NewGuid();
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        Guid locationId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        Result<StockLocation> stockLocationResult = StockLocationMethod.Create(
            name: "WH-Concurrency",
            active: true,
            id: locationId);
        stockLocationResult.IsSuccess.Should().BeTrue();
        db.Set<StockLocation>().Add(stockLocationResult.Value);

        Result<Product> productResult = ProductMethod.Create(
            name: "Concurrency Test Product",
            slug: "ctp-" + Guid.NewGuid().ToString("N")[..8],
            description: "concurrency test",
            status: ProductStatus.Active,
            availableOn: DateTimeOffset.UtcNow,
            id: productId);
        productResult.IsSuccess.Should().BeTrue();
        Product product = productResult.Value;
        product.MasterVariantId = variantId;
        db.Set<Product>().Add(product);

        Result<Variant> variantResult = VariantMethod.Create(
            productId: productId,
            sku: "CTP-" + Guid.NewGuid().ToString("N")[..8],
            isMaster: true,
            id: variantId);
        variantResult.IsSuccess.Should().BeTrue();
        db.Set<Variant>().Add(variantResult.Value);

        Result<StockItem> stockItemResult = StockItemMethod.Create(
            stockLocationId: locationId,
            variantId: variantId,
            backorderable: false,
            countOnHand: 1);
        stockItemResult.IsSuccess.Should().BeTrue();
        db.Set<StockItem>().Add(stockItemResult.Value);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (variantId, userA, userB, locationId);
    }

    private async Task<Guid> SeedDraftCartAsync(Guid userId, Guid variantId)
    {
        using IServiceScope scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Use the public OrderMethod.Create factory; then promote the
        // checkout state to Confirm and supply the address / shipping
        // fields that the CreateOrderFromCart handler validates.
        Result<Order> orderResult = OrderMethod.Create(
            currency: "USD",
            userId: userId,
            storeId: Guid.Empty,
            shipAddressId: Guid.NewGuid());
        orderResult.IsSuccess.Should().BeTrue();

        Order cart = orderResult.Value;
        cart.CheckoutState = CheckoutState.Confirm;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "u-" + userId.ToString("N")[..8] + "@test.local";

        db.Set<Order>().Add(cart);

        Result<LineItem> lineItemResult = LineItemMethod.Create(
            orderId: cart.Id,
            variantId: variantId,
            quantity: 1,
            price: 0m);
        lineItemResult.IsSuccess.Should().BeTrue();
        db.Set<LineItem>().Add(lineItemResult.Value);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return cart.Id;
    }

    private async Task<Result<CreateOrderFromCart.Response>> SendAsUserAsync(
        Guid userId,
        CreateOrderFromCart.Command command)
    {
        ApiFactory.TestCurrentUser.SetUser(userId);

        using IServiceScope scope = Fixture.Factory.Services.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            return await sender.Send(command, TestContext.Current.CancellationToken);
        }
        catch (DbUpdateException ex) when (IsSerializableConflict(ex))
        {
            // Serializable isolation may abort the second concurrent
            // commit with Postgres SQLSTATE 40001 (serialization_failure)
            // or 40P01 (deadlock_detected). The handler currently
            // re-throws, so we surface the conflict as a Result.Failure
            // here so the test can count it as "the loser".
            return Result<CreateOrderFromCart.Response>.Failure(Error.Conflict(
                code: "Ordering.Concurrency.SerializationConflict",
                message: "Concurrent checkout lost the serialization conflict."));
        }
    }

    private static bool IsSerializableConflict(DbUpdateException ex) =>
        ex.InnerException is Npgsql.PostgresException pg
        && (pg.SqlState == "40001" || pg.SqlState == "40P01");
}
