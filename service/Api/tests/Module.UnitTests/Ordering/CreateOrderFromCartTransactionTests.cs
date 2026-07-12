using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

using OrderEntity = Module.Ordering.Domain.Orders.Order;
using OrderItemEntity = Module.Ordering.Domain.LineItems.LineItem;

namespace Module.UnitTests.Ordering;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
public class CreateOrderFromCartTransactionTests
{
    [Fact(DisplayName = "CreateOrderFromCart: stock deduction commits inside a Serializable transaction")]
    public async Task Handle_StockDeduction_CommitsInsideTransaction()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OrderEntity).Assembly];
        var db = new ApplicationDbContext(opts);

        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var stockItemId = Guid.NewGuid();

        db.Set<StockLocation>().Add(new StockLocation { Id = locationId, Name = "WH-1", Active = true });
        var productId = Guid.NewGuid();
        db.Set<Product>().Add(new Product
        {
            Id = productId, Name = "X", Slug = "x", IsDeleted = false,
            AvailableOn = DateTimeOffset.UtcNow
        });
        db.Set<Variant>().Add(new Variant
        {
            Id = variantId, ProductId = productId, IsMaster = false, IsDeleted = false,
            Sku = "TEST-SKU"
        });
        db.Set<StockItem>().Add(new StockItem
        {
            Id = stockItemId, VariantId = variantId, StockLocationId = locationId,
            CountOnHand = 5, Backorderable = false
        });

        var cart = new OrderEntity
        {
            Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Draft,
            Number = "SEED", Currency = "USD", Email = "u@e.com",
            CheckoutState = CheckoutState.Confirm,
            BillAddressId = Guid.NewGuid(), ShipAddressId = Guid.NewGuid(),
            ShippingMethodId = Guid.NewGuid(),
            Total = 0m
        };
        db.Set<OrderEntity>().Add(cart);
        db.Set<OrderItemEntity>().Add(new OrderItemEntity
        {
            Id = Guid.NewGuid(), OrderId = cart.Id, VariantId = variantId, Quantity = 2
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = new Mock<ILogger<CreateOrderFromCart.CommandHandler>>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(userId.ToString());
        currentUser.Setup(x => x.UserName).Returns("tester");
        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var sut = new CreateOrderFromCart.CommandHandler(
            db, logger.Object, currentUser.Object, notificationService.Object);

        var result = await sut.Handle(
            new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var stockAfter = await db.Set<StockItem>()
            .SingleAsync(si => si.Id == stockItemId, TestContext.Current.CancellationToken);
        stockAfter.CountOnHand.Should().Be(3, "two units should be deducted from 5");
    }
}
