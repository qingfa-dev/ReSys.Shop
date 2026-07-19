# AvailabilityValidator Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Delete the cross-module `AvailabilityValidator` static class and replace its callers with ISender-based MediatR queries, fixing the module boundary violation and reservation-unaware stock check (RISK-006).

**Architecture:** A new Inventory `CheckStockAvailability` query wraps `IStockAvailabilityService.IsAvailableAnyLocationAsync` for `UpdateCartItemQuantity`. `AddToCart` drops its pre-check entirely — `ReserveCartStock` handler already does the authoritative serializable-transaction check. `AvailabilityValidator.cs` is deleted.

**Tech Stack:** .NET 10 C#, xUnit v3, FluentAssertions, EF Core InMemory, Moq, MediatR ISender

## Global Constraints

- `dotnet build` must pass with warnings-as-errors (0 new warnings)
- All existing Inventory + Ordering unit tests must pass
- No EF Core migration required (no schema change)
- Feature files follow vertical slice conventions: `static partial class`, split across Handler/Request/Response files
- All handlers return `Result<T>` or `Result` — no exceptions for domain errors
- Module boundary rule: Ordering must not `using Module.Inventory.Domain.Stock`

---

### Task 1: Create `CheckStockAvailability` query in Inventory

**Files:**
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Query.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Request.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Response.cs`

**Interfaces:**
- Produces: `CheckStockAvailability.Query(Guid VariantId, int Quantity)` implementing `IQuery<CheckStockAvailability.Response>`. Handler injected with `IStockAvailabilityService`. Returns `Response { bool IsAvailable, int TotalAvailable }`.
- Consumes: `IStockAvailabilityService` (already DI-registered in `Inventory.Extension.cs`)

This is an internal ISender query — no Endpoint file needed.

- [ ] **Step 1: Create Request file**

```bash
mkdir -p service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability
```

Write `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Request.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

public static partial class CheckStockAvailability
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
    }
}
```

- [ ] **Step 2: Create Response file**

Write `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Response.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

public static partial class CheckStockAvailability
{
    public sealed record Response
    {
        public Guid VariantId { get; init; }
        public bool IsAvailable { get; init; }
        public int TotalAvailable { get; init; }
    }
}
```

- [ ] **Step 3: Create Query + Handler file**

Write `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Query.cs`:

```csharp
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

/// <summary>Lightweight reservation-aware stock check for a variant — tolerates slightly stale reads for UX pre-validation.</summary>
public static partial class CheckStockAvailability
{
    public sealed record Query(Request Request) : IQuery<Response>;

    public sealed class QueryHandler(IStockAvailabilityService availabilityService)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var req = request.Request;

            if (req.Quantity <= 0)
                return new Response { VariantId = req.VariantId, IsAvailable = true, TotalAvailable = 0 };

            var isAvailable = await availabilityService.IsAvailableAnyLocationAsync(
                req.VariantId, req.Quantity, cancellationToken);

            return new Response
            {
                VariantId = req.VariantId,
                IsAvailable = isAvailable,
                TotalAvailable = 0
            };
        }
    }
}
```

- [ ] **Step 4: Build to verify compile**

Run: `dotnet build service/Api/src/Module/`
Expected: Build succeeded, 0 errors, 0 warnings

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/
git commit -m "feat(inventory): add CheckStockAvailability MediatR query"
```

---

### Task 2: Write unit tests for `CheckStockAvailability`

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Tests.cs`

**Interfaces:**
- Consumes: `CheckStockAvailability.QueryHandler` from Task 1
- Produces: Test coverage for `CheckStockAvailability` query — happy path, zero quantity, insufficient stock, no stock items

- [ ] **Step 1: Create test directory**

```bash
mkdir -p service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability
```

- [ ] **Step 2: Write the test file**

Write `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Tests.cs`:

```csharp
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;
using Module.Inventory.Services;

namespace Module.UnitTests.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "CheckStockAvailability")]
public class CheckStockAvailabilityTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CheckStockAvailability.QueryHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _locationId = Guid.NewGuid();

    public CheckStockAvailabilityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        var availabilityService = new StockAvailabilityService(_dbContext);
        _handler = new CheckStockAvailability.QueryHandler(availabilityService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "CheckStockAvailability: Should return available when stock is sufficient")]
    public async Task Handle_ShouldReturnAvailable_WhenStockSufficient()
    {
        var ct = TestContext.Current.CancellationToken;

        var stockItem = StockItemMethod.Create(stockLocationId: _locationId, variantId: _variantId, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CheckStockAvailability.Query(new CheckStockAvailability.Request { VariantId = _variantId, Quantity = 5 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeTrue();
        result.Value.VariantId.Should().Be(_variantId);
    }

    [Fact(DisplayName = "CheckStockAvailability: Should return unavailable when stock insufficient")]
    public async Task Handle_ShouldReturnUnavailable_WhenStockInsufficient()
    {
        var ct = TestContext.Current.CancellationToken;

        var stockItem = StockItemMethod.Create(stockLocationId: _locationId, variantId: _variantId, countOnHand: 3).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CheckStockAvailability.Query(new CheckStockAvailability.Request { VariantId = _variantId, Quantity = 10 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }

    [Fact(DisplayName = "CheckStockAvailability: Should return available for zero quantity")]
    public async Task Handle_ShouldReturnAvailable_WhenQuantityZero()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(
            new CheckStockAvailability.Query(new CheckStockAvailability.Request { VariantId = _variantId, Quantity = 0 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeTrue();
    }

    [Fact(DisplayName = "CheckStockAvailability: Should subtract active reservations")]
    public async Task Handle_ShouldSubtractActiveReservations()
    {
        var ct = TestContext.Current.CancellationToken;

        var stockItem = StockItemMethod.Create(stockLocationId: _locationId, variantId: _variantId, countOnHand: 5).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity: 3, state: ReservationState.Reserved,
            expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(10),
            stockLocationId: _locationId, orderId: Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CheckStockAvailability.Query(new CheckStockAvailability.Request { VariantId = _variantId, Quantity = 3 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }

    [Fact(DisplayName = "CheckStockAvailability: Should return unavailable when variant has no stock items")]
    public async Task Handle_ShouldReturnUnavailable_WhenNoStockItems()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(
            new CheckStockAvailability.Query(new CheckStockAvailability.Request { VariantId = Guid.NewGuid(), Quantity = 1 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }
}
```

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CheckStockAvailability"`
Expected: 5 passed, 0 failed, 0 skipped

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/
git commit -m "test(inventory): add unit tests for CheckStockAvailability query"
```

---

### Task 3: Modify `UpdateCartItemQuantity` — replace static call with ISender query

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs:1-69`

**Interfaces:**
- Consumes: `CheckStockAvailability.Query` from Task 1, `ISender` (injected into handler constructor)
- Produces: same `Result` return type, same `Command` signature

- [ ] **Step 1: Update `UpdateCartItemQuantity.cs`**

Read the current file at `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`.

Replace the entire file:

```csharp
using Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;
/// <summary>Updates the quantity of a line item in the current user's draft cart after validating stock availability.</summary>
public static partial class UpdateCartItemQuantity
{
    public sealed record Command(Guid LineItemId, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        ISender sender)
        : ICommandHandler<Command>
    {
        /// <summary>Validates stock via Inventory module, updates the line item quantity and total, and recalculates cart totals.</summary>
        /// <param name="command">The command containing the line item ID and new quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            if (command.Request.Quantity <= 0 || command.Request.Quantity > LineItemConstant.MaxQuantity)
                return OrderResult.Errors.QuantityNotPositive;

            // Check: Find the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Validate: Stock availability via Inventory module's reservation-aware query.
            var stockResult = await sender.Send(
                new CheckStockAvailability.Query(new CheckStockAvailability.Request
                {
                    VariantId = lineItem.VariantId,
                    Quantity = command.Request.Quantity
                }),
                cancellationToken);

            if (stockResult.IsFailure)
                return stockResult.Errors;

            if (!stockResult.Value.IsAvailable)
                return StockItemResult.Errors.InsufficientStock;

            // Update: Modify quantity and total.
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Errors;
            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record quantity change in audit log.
            LineItemLoggers.QuantityUpdated(logger, Id: lineItem.Id, OrderId: cart.Id, Quantity: lineItem.Quantity, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
```

The changes from the original:
1. Added `using Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;` (replaces `using Module.Inventory.Domain.Stock;` and `using Module.Inventory.Domain.StockLocations.StockItems;`)
2. Added `ISender sender` to `CommandHandler` constructor parameters
3. Replaced lines 45–51 (stockItems query + `AvailabilityValidator.IsAvailable` call) with the `sender.Send(CheckStockAvailability.Query)` call
4. Removed `StockItemResult.Errors.InsufficientStock` inline and replaced with the response check `!stockResult.Value.IsAvailable`

- [ ] **Step 2: Build to verify compile**

Run: `dotnet build service/Api/src/Module/`
Expected: Build succeeded, 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs
git commit -m "refactor(ordering): replace AvailabilityValidator with CheckStockAvailability query in UpdateCartItemQuantity"
```

---

### Task 4: Update `UpdateCartItemQuantity` tests

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs`

**Interfaces:**
- Consumes: Updated `UpdateCartItemQuantity.CommandHandler` from Task 3 (now needs `ISender` mock)
- Produces: Same test coverage, adapted for ISender mock

- [ ] **Step 1: Read current test file**

Read `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs` to verify current content matches expectations.

- [ ] **Step 2: Update the test file**

Replace `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs`:

```csharp
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateCartItemQuantity")]
public class UpdateCartItemQuantityTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<UpdateCartItemQuantity.CommandHandler>> _loggerMock;
    private readonly UpdateCartItemQuantity.CommandHandler _handler;
    private readonly Guid _userId;
    private readonly Guid _variantId;
    private readonly Guid _lineItemId;

    public UpdateCartItemQuantityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(StockItem).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _variantId = Guid.NewGuid();
        _lineItemId = Guid.NewGuid();

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _senderMock = new Mock<ISender>();
        _loggerMock = new Mock<ILogger<UpdateCartItemQuantity.CommandHandler>>();
        _handler = new UpdateCartItemQuantity.CommandHandler(
            _dbContext, _loggerMock.Object, _currentUserMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update item quantity")]
    public async Task Handle_ShouldUpdateQuantity_WhenItemExists()
    {
        // Arrange: Seed cart with line item
        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = _lineItemId,
            OrderId = cart.Id,
            VariantId = _variantId,
            Quantity = 2,
            Price = 19.99m,
            Total = 39.98m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(x => x.Send(
                It.Is<CheckStockAvailability.Query>(
                    q => q.Request.VariantId == _variantId && q.Request.Quantity == 5),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CheckStockAvailability.Response>.Ok(
                new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = true, TotalAvailable = 10 }));

        // Act
        var result = await _handler.Handle(
            new UpdateCartItemQuantity.Command(_lineItemId, new UpdateCartItemQuantity.Request { Quantity = 5 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var lineItem = await _dbContext.Set<Module.Ordering.Domain.LineItems.LineItem>()
            .FirstAsync(li => li.Id == _lineItemId, TestContext.Current.CancellationToken);
        lineItem.Quantity.Should().Be(5);
        lineItem.Total.Should().Be(19.99m * 5);
    }

    [Fact(DisplayName = "Handler: Should fail when quantity exceeds stock")]
    public async Task Handle_ShouldFail_WhenInsufficientStock()
    {
        // Arrange: Seed cart with line item
        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = _lineItemId,
            OrderId = cart.Id,
            VariantId = _variantId,
            Quantity = 1,
            Price = 19.99m,
            Total = 19.99m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(x => x.Send(
                It.Is<CheckStockAvailability.Query>(
                    q => q.Request.VariantId == _variantId && q.Request.Quantity == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CheckStockAvailability.Response>.Ok(
                new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = false, TotalAvailable = 3 }));

        // Act
        var result = await _handler.Handle(
            new UpdateCartItemQuantity.Command(_lineItemId, new UpdateCartItemQuantity.Request { Quantity = 10 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when line item not found in cart")]
    public async Task Handle_ShouldFail_WhenItemNotFound()
    {
        // Arrange: Create cart but no matching line item
        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new UpdateCartItemQuantity.Command(Guid.NewGuid(), new UpdateCartItemQuantity.Request { Quantity = 1 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
```

Key changes from the original test:
1. Added `Mock<ISender> _senderMock` field and passed it to handler constructor
2. Tests no longer seed `StockItem` entities (handler no longer queries `StockItem` directly)
3. Added ISender mock setup for each test that reaches the stock check
4. `Handle_ShouldFail_WhenItemNotFound` needs no ISender setup (returns early before stock check)

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~UpdateCartItemQuantity"`
Expected: 3 passed, 0 failed, 0 skipped

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs
git commit -m "test(ordering): update UpdateCartItemQuantity tests for ISender-based stock check"
```

---

### Task 5: Modify `AddToCart` — remove `AvailabilityValidator`, keep `ReserveCartStock`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs:1-158`

**Interfaces:**
- Consumes: `ReserveCartStock.Command` (unchanged), `ISender` (already injected)
- Produces: same `Command`/`Response` types, same `ICommandHandler<Command, Response>` contract

- [ ] **Step 1: Update `AddToCart.cs`**

Read the current file at `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`.

Replace the entire file:

```csharp
using Module.Catalog.Domain.Products.Variants;

using Shared.Application.Systems.SystemInfos;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Storefront.CartReservations.Reserve;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

/// <summary>Adds a variant to the current user's cart, creating a new draft order if none exists, merging with existing line items for the same variant.</summary>
public static partial class AddToCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        ISystemInfo systemInfo,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Adds a variant to the user's cart, creating a new cart or merging with an existing line item, with stock reservation.</summary>
        /// <param name="command">The command containing the variant ID and quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response with the new or updated line item ID.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var request = command.Request;

            // Check: Resolve current user identifier or guest session.
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Variant exists in catalog — reject unknown products.
            var variant = await dbContext.Set<Variant>()
                .FirstOrDefaultAsync(x => x.Id == request.VariantId, cancellationToken);

            if (variant is null)
                return LineItemResult.Errors.VariantNotFound(request.VariantId);

            // Check: Find or create draft order for current user or guest session.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
            {
                // Create: New draft cart with default currency from system info.
                var currency = systemInfo.DefaultCurrency;
                var createResult = OrderMethod.Create(currency, userId, Guid.Empty, sessionId: sessionId, shipAddressId: null);
                if (createResult.IsFailure)
                    return createResult.Errors;

                cart = createResult.Value;
                dbContext.Set<Order>().Add(cart);
            }

            // Reserve: Find the best location with stock and reserve via Inventory module.
            var primaryLocation = await dbContext.Set<StockItem>()
                .Where(si => si.VariantId == request.VariantId && si.CountOnHand > 0)
                .OrderByDescending(si => si.CountOnHand)
                .FirstOrDefaultAsync(cancellationToken);

            if (primaryLocation is not null)
            {
                var cartToken = currentUser.IsAuthenticated
                    ? currentUser.UserId!
                    : currentUser.SessionId ?? string.Empty;

                const int CartReservationTtlMinutes = 30;
                var reserveResult = await sender.Send(
                    new ReserveCartStock.Command(
                        new ReserveCartStock.Request
                        {
                            VariantId = request.VariantId,
                            Quantity = request.Quantity,
                            StockLocationId = primaryLocation.StockLocationId,
                            TtlMinutes = CartReservationTtlMinutes,
                            CartToken = cartToken
                        }),
                    cancellationToken);

                if (reserveResult.IsFailure)
                    return reserveResult.Errors;
            }

            // Merge: Variant already in cart — add to existing line item quantity.
            var existingLine = cart.LineItems.FirstOrDefault(li => li.VariantId == request.VariantId);
            if (existingLine is not null)
            {
                // Validate: Combined quantity must not exceed per-line maximum.
                if (existingLine.Quantity + request.Quantity > LineItemConstant.MaxQuantity)
                    return LineItemResult.Errors.QuantityExceedsMax;
                // Update: Increment existing line item quantity and recalculate.
                var updateResult = existingLine.UpdateQuantity(existingLine.Quantity + request.Quantity);
                if (updateResult.IsFailure)
                    return updateResult.Errors;
                var recalcResult = cart.RecalculateTotals();
                if (recalcResult.IsFailure)
                    return recalcResult.Errors;
                await dbContext.SaveChangesAsync(cancellationToken);
                var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
                var variantNames = await dbContext.Set<Variant>()
                    .Where(v => variantIds.Contains(v.Id))
                    .AsNoTracking()
                    .ToDictionaryAsync(v => v.Id, v => v.Sku ?? "", cancellationToken);
                return Result<Response>.Ok(cart.MapToDetailWithItems<Response>(variantNames));
            }

            // Create: Add new line item to cart with variant price snapshot.
            var lineItem = LineItemMethod.Create(cart.Id, request.VariantId, request.Quantity, variant.Price ?? 0);
            if (lineItem.IsFailure)
                return lineItem.Errors;

            var newItem = lineItem.Value;

            dbContext.Set<LineItem>().Add(newItem);
            var addRecalcResult = cart.RecalculateTotals();
            if (addRecalcResult.IsFailure)
                return addRecalcResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record the new line item in audit log.
            LineItemLoggers.Created(logger, Id: newItem.Id, OrderId: cart.Id, VariantId: request.VariantId, ActionBy: currentUser.UserName);

            var allVariantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var allVariantNames = await dbContext.Set<Variant>()
                .Where(v => allVariantIds.Contains(v.Id))
                .AsNoTracking()
                .ToDictionaryAsync(v => v.Id, v => v.Sku ?? "", cancellationToken);

            return Result<Response>.Created(
                cart.MapToDetailWithItems<Response>(allVariantNames),
                LineItemResult.Success.Created(newItem.Id));
        }
    }
}
```

The changes from the original:
1. Removed `using Module.Inventory.Domain.Stock;` (line 4 — `AvailabilityValidator` namespace)
2. Removed lines 69–76 (stockItems load + `IsAvailable` pre-check)
3. Replaced lines 79–82 (primary location computation from in-memory list) with a single `FirstOrDefaultAsync` query (no `Include` needed — only `StockLocationId` and `CountOnHand` are accessed)
4. Lines 84–105 (`ReserveCartStock` dispatch) remain functionally identical

- [ ] **Step 2: Build to verify compile**

Run: `dotnet build service/Api/src/Module/`
Expected: Build succeeded, 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs
git commit -m "refactor(ordering): remove AvailabilityValidator pre-check from AddToCart — rely on ReserveCartStock"
```

---

### Task 6: Update `AddToCart.Reservation.Tests`

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Reservation.Tests.cs`

**Interfaces:**
- Consumes: Updated `AddToCart.CommandHandler` from Task 5 (unchanged handler constructor, but handler's internal logic changed — no more `IsAvailable` check)
- Produces: Same test coverage, adapted for new handler behavior

- [ ] **Step 1: Read current test file**

Read `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Reservation.Tests.cs` to verify current content.

- [ ] **Step 2: Update the test file**

Replace `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Reservation.Tests.cs`:

```csharp
using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.CartReservations.Reserve;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.AddItem;

using Shared.Application.Systems.SystemInfos;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AddItem;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AddToCart.Reservation")]
public class AddToCartReservationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ILogger<AddToCart.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ISystemInfo> _systemInfoMock;
    private readonly AddToCart.CommandHandler _handler;

    public AddToCartReservationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly, typeof(StockItem).Assembly, typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _loggerMock = new Mock<ILogger<AddToCart.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _systemInfoMock = new Mock<ISystemInfo>();
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _handler = new AddToCart.CommandHandler(
            _dbContext, _loggerMock.Object, _currentUserMock.Object, _systemInfoMock.Object, _senderMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "AddToCart: Dispatches ReserveCartStock when stock location exists")]
    public async Task Handle_ShouldDispatchReserveCartStock_WhenStockLocationExists()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var variant = new Variant { Id = variantId, Sku = "SKU-001", Price = 9.99m };
        _dbContext.Set<Variant>().Add(variant);

        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync();

        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserName).Returns("test");
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _senderMock
            .Setup(x => x.Send(
                It.Is<ReserveCartStock.Command>(c => c.Request.VariantId == variantId && c.Request.Quantity == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveCartStock.Response>.Ok(new ReserveCartStock.Response
            {
                Id = Guid.NewGuid(),
                VariantId = variantId,
                Quantity = 1,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                State = "Reserved"
            }));

        var request = new AddToCart.Request { VariantId = variantId, Quantity = 1 };

        var result = await _handler.Handle(
            new AddToCart.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _senderMock.Verify(
            x => x.Send(
                It.Is<ReserveCartStock.Command>(c => c.Request.VariantId == variantId && c.Request.Quantity == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "AddToCart: Returns failure when reservation fails")]
    public async Task Handle_ShouldReturnFailure_WhenReservationFails()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var variant = new Variant { Id = variantId, Sku = "SKU-002", Price = 9.99m };
        _dbContext.Set<Variant>().Add(variant);

        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync();

        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 1).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserName).Returns("test");
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _senderMock
            .Setup(x => x.Send(
                It.Is<ReserveCartStock.Command>(c => c.Request.VariantId == variantId && c.Request.Quantity == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockReservationResult.Errors.InsufficientStock);

        var request = new AddToCart.Request { VariantId = variantId, Quantity = 1 };

        var result = await _handler.Handle(
            new AddToCart.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }
}
```

Key changes from original:
1. Test names updated (removed "stock validation" phrasing — `AvailabilityValidator` is gone)
2. Both tests still pass — the first test has `countOnHand: 10` so the `FirstOrDefaultAsync` query returns the `StockItem`, and the `ReserveCartStock` mock returns success. The second test has `countOnHand: 1` and the `ReserveCartStock` mock returns `InsufficientStock`.
3. Removed unused `using` for `Module.Inventory.Domain.StockReservations` — but kept it because `StockReservationResult.Errors.InsufficientStock` is used in test 2.

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~AddToCart.Reservation"`
Expected: 2 passed, 0 failed, 0 skipped

- [ ] **Step 3: Run full AddToCart test suite to ensure no regressions**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~AddToCart"`
Expected: all passed, 0 failed

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Reservation.Tests.cs
git commit -m "test(ordering): update AddToCart reservation tests for AvailabilityValidator removal"
```

---

### Task 7: Delete `AvailabilityValidator.cs`

**Files:**
- Delete: `service/Api/src/Module/Inventory/Domain/Stock/AvailabilityValidator.cs`

**Interfaces:**
- Produces: nothing (deletion)
- Consumes: no callers remain after Tasks 3 and 5

- [ ] **Step 1: Verify no remaining callers**

Run:
```bash
grep -r "AvailabilityValidator" service/Api/src/ --include="*.cs"
```
Expected: Only hits in `AvailabilityValidator.cs` itself (the file being deleted). No hits in Ordering module.

- [ ] **Step 2: Delete the file**

```bash
rm service/Api/src/Module/Inventory/Domain/Stock/AvailabilityValidator.cs
```

- [ ] **Step 3: Verify `using Module.Inventory.Domain.Stock` is gone from Ordering**

Run:
```bash
grep -r "using Module.Inventory.Domain.Stock" service/Api/src/Module/Ordering/ --include="*.cs"
```
Expected: No output (empty).

- [ ] **Step 4: Build to verify compile**

Run: `dotnet build service/Api/src/Module/`
Expected: Build succeeded, 0 errors, 0 warnings

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Inventory/Domain/Stock/AvailabilityValidator.cs
git commit -m "refactor(inventory): delete AvailabilityValidator — replaced by MediatR queries"
```

---

### Task 8: Full test suite validation and final commit

**Files:**
- None modified (verification only)

- [ ] **Step 1: Run full Inventory test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Inventory"`
Expected: 0 failures

- [ ] **Step 2: Run full Ordering test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`
Expected: 0 failures

- [ ] **Step 3: Run full test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: 0 failures, no new skipped tests beyond the 3 pre-existing skips

- [ ] **Step 4: Run full build with warnings-as-errors**

Run: `dotnet build`
Expected: Build succeeded, 0 errors, 0 warnings

- [ ] **Step 5: Final commit (amend the last commit to keep history clean)**

```bash
git log --oneline -5
```
Review that all 7 commits are in order. If any tests failed and required fixes, squash the fix commits into the relevant task commit.

---

## Self-Review

### Spec Coverage Check

| Spec Requirement | Covered By |
|---|---|
| BND-001: Delete `AvailabilityValidator` | Task 7 |
| BND-002: `AddToCart` must not `using Module.Inventory.Domain.Stock` | Task 5 (removed line 4), Task 7 Step 3 (grep verification) |
| BUG-001: `IsAvailable` (reservation-unaware) removed | Task 7 (deleted), Task 5 (removed caller) |
| DES-001: AddToCart pre-check removed, rely on ReserveCartStock | Task 5 |
| DES-002: UpdateCartItemQuantity — new CheckStockAvailability query via ISender | Task 3 (handler), Task 1 (query), Task 2 (query tests) |
| DES-003: Lightweight query tolerates stale reads | Task 1 (IStockAvailabilityService, no serializable tx) |

### Placeholder Scan
- No "TBD", "TODO", or "implement later"
- No "add error handling" without code
- No "similar to Task N" references
- All code steps contain actual code

### Type Consistency
- `CheckStockAvailability.Query(Request Request)` matches across Task 1 definition and Task 3/4 usage
- `CheckStockAvailability.Response { IsAvailable, TotalAvailable, VariantId }` matches across all references
- `ISender` injection in `CommandHandler` constructors consistent across Tasks 3 and 5
- `StockItemResult.Errors.InsufficientStock` used consistently — its type is `Error` which implicitly converts to `Result` and `Result<T>`
