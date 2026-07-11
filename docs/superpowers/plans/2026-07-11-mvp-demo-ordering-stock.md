# MVP Demo Ordering & Stock Integrity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the cart-to-order flow demo-safe by preventing overselling, removing hardcoded defaults, and honoring product discontinuation status.

**Architecture:** Replace the two-step stock check/deduct pattern with atomic `ExecuteUpdateAsync` operations, push cart creation defaults into configuration/factory parameters, and query the catalog for discontinued variants inside the ordering domain guard.

**Tech Stack:** .NET 10, EF Core, Npgsql, Carter minimal APIs, MediatR, xUnit, FluentAssertions

## Global Constraints

- All domain operations return `Result<T>` or `Result`; exceptions only for unrecoverable infrastructure failures.
- Modules never reference each other; communication via MediatR `ISender` only.
- Every C# feature action is a `static partial class` split across files.
- `TreatWarningsAsErrors=true` globally.

---

### Task 1: Atomic Stock Deduction in `CreateOrderFromCart`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`
- Create: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.StockService.cs` (optional helper)
- Test: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartStockTests.cs` (create)

**Interfaces:**
- Consumes: `StockItem` entity, `ExecuteUpdateAsync`
- Produces: atomic decrement with insufficient-stock guard

- [ ] **Step 1: Write the failing concurrent-stock test**

```csharp
using FluentAssertions;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;
using Shared.Testing;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Checkout;

public class CreateOrderFromCartStockTests : TestBase
{
    [Fact]
    public async Task Handle_Concurrent_Checkouts_Should_Not_Oversell()
    {
        // This test requires PostgreSQL; skip under InMemory if needed.
        var variant = await CreateVariantAsync();
        var location = await CreateStockLocationAsync();
        await CreateStockItemAsync(variant.Id, location.Id, countOnHand: 1);
        var cart1 = await CreateCartWithItemAsync(variant.Id, quantity: 1);
        var cart2 = await CreateCartWithItemAsync(variant.Id, quantity: 1);

        var task1 = SendAsync(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()));
        var task2 = SendAsync(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()));

        var results = await Task.WhenAll(task1, task2);
        var successes = results.Count(r => r.IsSuccess);

        successes.Should().BeLessThanOrEqualTo(1);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~CreateOrderFromCartStockTests"`

Expected: FAIL or skipped under InMemory

- [ ] **Step 3: Replace check-then-deduct with atomic ExecuteUpdateAsync**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`, replace the stock validation loop (lines 89-99) and the deduction loop (lines 107-141) with a single atomic pass:

```csharp
foreach (var lineItem in cart.LineItems)
{
    var stockItems = await dbContext.Set<StockItem>()
        .Where(si => si.VariantId == lineItem.VariantId)
        .OrderByDescending(si => si.CountOnHand)
        .ToListAsync(cancellationToken);

    var remaining = lineItem.Quantity;
    foreach (var si in stockItems)
    {
        if (remaining <= 0) break;
        var take = Math.Min(si.CountOnHand, remaining);
        if (take <= 0) continue;

        var updated = await dbContext.Set<StockItem>()
            .Where(x => x.Id == si.Id && x.CountOnHand >= take)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.CountOnHand, x => x.CountOnHand - take)
                .SetProperty(x => x.ModifiedAtUtc, DateTimeOffset.UtcNow),
                cancellationToken);

        if (updated == 0)
            return StockItemResult.Errors.InsufficientStock;

        remaining -= take;

        var reservation = StockReservationMethod.Reserve(
            si.VariantId, take, si.StockLocationId, cart.Id, 30).Value;
        dbContext.Set<StockReservation>().Add(reservation);

        var movementResult = StockMovementMethod.Create(
            stockItemId: si.Id,
            quantity: -take,
            previousCountOnHand: si.CountOnHand,
            originatorType: "Order",
            originatorId: cart.Id,
            action: "ship",
            createdBy: currentUser.UserName);

        if (movementResult.IsSuccess)
            dbContext.Set<StockMovement>().Add(movementResult.Value);
    }

    if (remaining > 0)
        return StockItemResult.Errors.InsufficientStock;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~CreateOrderFromCartStockTests"`

Expected: PASS (may require PostgreSQL integration test project)

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartStockTests.cs
git commit -m "feat(ordering): atomic stock deduction in CreateOrderFromCart"
```

---

### Task 2: Remove Hardcoded Cart Defaults

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Extensions.cs` (or factory)
- Modify: `service/Api/src/Api/appsettings.Development.json` and `appsettings.json`
- Test: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCartDefaultsTests.cs` (create)

**Interfaces:**
- Consumes: `IConfiguration["Ordering:DefaultCurrency"]`
- Produces: carts created with configured currency and no default address

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.AddItem;
using Shared.Testing;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AddItem;

public class AddToCartDefaultsTests : TestBase
{
    [Fact]
    public async Task Handle_Should_Create_Cart_With_Configured_Currency()
    {
        var variant = await CreateVariantAsync();

        var result = await SendAsync(new AddToCart.Command(
            new AddToCart.Request { VariantId = variant.Id, Quantity = 1 }));

        result.IsSuccess.Should().BeTrue();
        var cart = await DbContext.Set<Order>().FindAsync(result.Value.OrderId);
        cart.Should().NotBeNull();
        cart!.Currency.Should().Be("USD"); // configured in test fixture
        cart.ShipAddressId.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~AddToCartDefaultsTests"`

Expected: FAIL — `ShipAddressId` is `Guid.Empty`

- [ ] **Step 3: Add default currency configuration**

In `service/Api/src/Api/appsettings.Development.json` and `appsettings.json`:

```json
"Ordering": {
  "DefaultCurrency": "USD",
  "CartExpiry": { "AfterDays": 7 }
}
```

- [ ] **Step 4: Update AddToCart to use configuration and nullable address**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`, inject `IConfiguration` (or `IOptions<OrderingSetting>`) and change cart creation:

```csharp
var currency = configuration["Ordering:DefaultCurrency"] ?? "USD";
var createResult = OrderExtensions.Create(currency, userId, shipAddressId: null, sessionId: sessionId);
```

Update `OrderExtensions.Create` signature if it requires a non-nullable `Guid` for ship address:

```csharp
public static Result<Order> Create(string currency, Guid? userId, Guid? shipAddressId, string? sessionId)
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~AddToCartDefaultsTests"`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Extensions.cs
git add service/Api/src/Api/appsettings.json
git add service/Api/src/Api/appsettings.Development.json
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCartDefaultsTests.cs
git commit -m "feat(ordering): use configured currency and nullable address in cart creation"
```

---

### Task 3: Implement `AssignDefaultAddresses`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` (or caller)
- Create: `service/Api/src/Module/Profile/Features/Store/Addresses/GetDefault/GetDefaultAddress.cs` (if missing)
- Test: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderCheckoutTests.cs` (create)

**Interfaces:**
- Consumes: `IMediator`, default-address query from Profile module
- Produces: `Order.BillAddressId` and `Order.ShipAddressId` populated when defaults exist

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Ordering.Domain.Orders;
using Shared.Testing;

namespace Module.UnitTests.Ordering.Domain.Orders;

public class OrderCheckoutTests : TestBase
{
    [Fact]
    public void AssignDefaultAddresses_Should_Set_Addresses_When_Defaults_Exist()
    {
        var order = OrderExtensions.Create("USD", Guid.NewGuid(), null, null).Value;
        var billId = Guid.NewGuid();
        var shipId = Guid.NewGuid();

        order.AssignDefaultAddresses(billId, shipId);

        order.BillAddressId.Should().Be(billId);
        order.ShipAddressId.Should().Be(shipId);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~OrderCheckoutTests"`

Expected: FAIL — method has no parameters / is empty

- [ ] **Step 3: Implement the domain method**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs`:

```csharp
internal void AssignDefaultAddresses(Guid? billAddressId, Guid? shipAddressId)
{
    if (BillAddressId is null && billAddressId is not null)
        BillAddressId = billAddressId;

    if (ShipAddressId is null && shipAddressId is not null)
        ShipAddressId = shipAddressId;
}
```

- [ ] **Step 4: Wire caller to query Profile defaults**

In the handler that advances checkout to the address step (likely `UpdateCheckout.cs`), dispatch a query:

```csharp
var defaultBill = await mediator.Send(new GetDefaultBillAddress.Query(userId), ct);
var defaultShip = await mediator.Send(new GetDefaultShipAddress.Query(userId), ct);

order.AssignDefaultAddresses(
    defaultBill.IsSuccess ? defaultBill.Value.Id : null,
    defaultShip.IsSuccess ? defaultShip.Value.Id : null);
```

If Profile does not yet expose these queries, create them as vertical slices in `service/Api/src/Module/Profile/Features/Store/Addresses/GetDefault/`.

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~OrderCheckoutTests"`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs
git add service/Api/src/Module/Profile/Features/Store/Addresses/GetDefault/
git add service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderCheckoutTests.cs
git commit -m "feat(ordering): assign default addresses from profile"
```

---

### Task 4: Fix `EnsureLineItemVariantsAreNotDiscontinued`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` (pass catalog data)
- Test: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderDiscontinuedTests.cs` (create)

**Interfaces:**
- Consumes: set of discontinued `VariantId`s from caller
- Produces: false when any line item variant is discontinued

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Shared.Testing;

namespace Module.UnitTests.Ordering.Domain.Orders;

public class OrderDiscontinuedTests : TestBase
{
    [Fact]
    public void EnsureLineItemVariantsAreNotDiscontinued_Should_Return_False_When_Variant_Discontinued()
    {
        var order = OrderExtensions.Create("USD", Guid.NewGuid(), null, null).Value;
        var variantId = Guid.NewGuid();
        order.LineItems.Add(LineItemMethod.Create(order.Id, variantId, 1, 10.00m).Value);

        var result = order.EnsureLineItemVariantsAreNotDiscontinued(new HashSet<Guid> { variantId });

        result.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~OrderDiscontinuedTests"`

Expected: FAIL — method always returns true

- [ ] **Step 3: Implement the check**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs`:

```csharp
internal bool EnsureLineItemVariantsAreNotDiscontinued(HashSet<Guid> discontinuedVariantIds)
{
    return !LineItems.Any(li => discontinuedVariantIds.Contains(li.VariantId));
}
```

- [ ] **Step 4: Update caller to supply discontinued IDs**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`, before placing the order:

```csharp
var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
var discontinuedVariantIds = await dbContext.Set<Variant>()
    .Where(v => variantIds.Contains(v.Id) && v.IsDiscontinued)
    .Select(v => v.Id)
    .ToHashSetAsync(cancellationToken);

if (!cart.EnsureLineItemVariantsAreNotDiscontinued(discontinuedVariantIds))
    return OrderResult.Errors.VariantDiscontinued;
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~OrderDiscontinuedTests"`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git add service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderDiscontinuedTests.cs
git commit -m "feat(ordering): enforce discontinued variant check at checkout"
```

---

### Task 5: Final Verification

- [ ] **Step 1: Run Module unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Ordering"`

Expected: All ordering tests pass

- [ ] **Step 2: Run build**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 3: Commit**

```bash
git commit -m "chore(ordering): final verification for ordering and stock fixes" --allow-empty
```
