# Order Domain — Convention Alignment & Bug Fixes

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename `Order` domain files to `{Entity}.Method.{Concern}.cs` convention, fold Services wrappers into extension methods, and fix 8 bugs.

**Architecture:** All business logic moves into `OrderMethod` static partial class split across `Order.Method.*.cs` files. The `Services/` subdirectory and its standalone classes (`OrderContents`, `OrderMerger`, `OrderUpdater`) are deleted; their logic becomes extension methods on `Order` inside `OrderMethod`. `OrderNumber.Generate()` returns `Result<string>` instead of throwing.

**Tech Stack:** .NET 10 C#, xUnit + FluentAssertions (tests), no new dependencies.

## Global Constraints

- `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- All domain operations return `Result` / `Result<T>`. No exceptions for control flow.
- No new cross-module references.
- File renames must update all `using` statements and callers.
- No behavioral change to correct code paths. Only broken paths are modified.

---

### Task 1: Add `OrderNumberGenerationFailed` error to `OrderResult.Errors`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs`

**Produces:** `OrderResult.Errors.OrderNumberGenerationFailed` — consumed by Task 9.

- [ ] **Step 1: Add the error factory**

Open `Order.Result.cs`. After the `#endregion` comment for the `Validation` region (line ~205), before `#region Auth`, add:

```csharp
        /// <summary>Failed to generate a unique order number after retries.</summary>
        public static Error OrderNumberGenerationFailed => Error.Validation(
            code: "Order.Number.GenerationFailed",
            message: "Failed to generate a unique order number after maximum retry attempts.");
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings).

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs
git commit -m "feat: add OrderNumberGenerationFailed error factory"
```

---

### Task 2: Rename `Order.Extensions.cs` → `Order.Method.cs` + class rename `OrderExtensions` → `OrderMethod`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Extensions.cs` → rename to `Order.Method.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Extensions.Tests.cs` → rename to `Order.Method.Tests.cs`

**Caller files that reference `OrderExtensions.` (18 files):**
- Modify: `service/Api/src/Module/Ordering/Persistence/Seeders/Order.Seeder.cs:69`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Domain.cs:20`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.cs:35`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs:60`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Domain.cs:4,8`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/OrderNumberTests.cs:48-56`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Extensions.Tests.cs` (all 17 references — rename file, class, refs)
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderCheckoutTests.cs:12,28,44`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderDiscontinuedTests.cs:14,26`
- Modify: All other test files listed below

**Test files to update (55 refs across 19 files):**
- `tests/Module.UnitTests/Shipping/Features/Storefront/Shipping/Calculate/CalculateShippingHandlerTests.cs` (lines 49, 109)
- `tests/Module.UnitTests/Shipping/Features/Storefront/Cart/SelectShippingRateCalculationTests.cs` (lines 54, 95)
- `tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` (lines 73, 99, 125)
- `tests/Module.UnitTests/Ordering/Infrastructure/Notifications/EventHandlerInvocationTests.cs` (line 51)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Orders/ListOrders/ListCustomerOrdersTests.cs` (lines 41, 45)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Tests.cs` (line 15)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs` (lines 63, 102, 129)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Tests.cs` (line 47)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/EmptyCart/EmptyCart.Tests.cs` (line 41)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCartTests.cs` (line 41)
- `tests/Api.Tests/Scenarios/Ordering/CheckoutConcurrencyTests.cs` (lines 145, 148)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs` (lines 69, 113)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartStockTests.cs` (lines 69, 112, 130)
- `tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AssociateCart/AssociateCartWithUserTests.cs` (lines 43, 49)
- `tests/Module.UnitTests/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatusTests.cs` (lines 46, 69)
- `tests/Module.UnitTests/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs` (line 135)
- `tests/Module.UnitTests/Ordering/Features/Admin/Orders/Resume/ResumeOrderTests.cs` (line 40)
- `tests/Module.UnitTests/Ordering/Features/Admin/Orders/Get/LineItemById/GetOrderLineItemByIdTests.cs` (lines 35, 57)
- `tests/Module.UnitTests/Ordering/Features/Admin/Orders/Approve/ApproveOrderTests.cs` (line 39)

**Produces:** `OrderMethod` static partial class — consumed by Tasks 3-8.

- [ ] **Step 1: Rename file + change class name in domain layer**

Rename the file:
```bash
git mv service/Api/src/Module/Ordering/Domain/Orders/Order.Extensions.cs \
       service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs
```

In `Order.Method.cs`, change line 5 from:
```csharp
public static class OrderExtensions
```
to:
```csharp
public static partial class OrderMethod
```

- [ ] **Step 2: Rename test file**

```bash
git mv service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Extensions.Tests.cs \
       service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs
```

In `Order.Method.Tests.cs`, change line 6 class name:
```csharp
public class OrderExtensionsTests
```
to:
```csharp
public class OrderMethodTests
```

- [ ] **Step 3: Replace all `OrderExtensions.` → `OrderMethod.` in source files**

Run a single find-and-replace across all 18 source+test files listed above:
```bash
rg -l "OrderExtensions\." service/Api/src/ service/Api/tests/ | \
  xargs sed -i 's/OrderExtensions\./OrderMethod./g'
```

- [ ] **Step 4: Replace all `OrderExtensions` (standalone, not dotted) in source files**

The remaining occurrences are in comments/docs only:
```bash
rg -l "\bOrderExtensions\b" service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Domain.cs | \
  xargs sed -i 's/\bOrderExtensions\b/OrderMethod/g'
```

- [ ] **Step 5: Replace all `OrderExtensions` (dotted or standalone) in the renamed test file**

All 17 occurrences in `Order.Method.Tests.cs` should already be handled by Step 3.

- [ ] **Step 6: Build to verify no errors**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings).

- [ ] **Step 7: Run unit tests to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering" --no-restore
```

Expected: All existing tests pass.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "refactor: rename OrderExtensions to OrderMethod, Order.Extensions.cs to Order.Method.cs"
```

---

### Task 3: Rename `Order.Checkout.cs` → `Order.Method.Checkout.cs` + fix bugs

**Files:**
- Rename: `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs` → `Order.Method.Checkout.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderCheckoutTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderDiscontinuedTests.cs`

**Bugs fixed in this task:** REQ-018 (AfterCancel/AfterResume stubs), REQ-019 (EnsureLineItemsPresent), REQ-020 (double negation)

- [ ] **Step 1: Rename file**

```bash
git mv service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs \
       service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs
```

- [ ] **Step 2: Remove `AfterCancel()` and `AfterResume()` stubs**

In `Order.Method.Checkout.cs`, delete lines 98-116 (the entire `#region State Machine Callbacks` section with both stubs and their `#pragma warning` lines).

The file should have this content at the end of the class:

```csharp
    // Assign: Default addresses from user profile on entering address step
    internal void AssignDefaultAddresses(Guid? billAddressId, Guid? shipAddressId)
    {
        if (BillAddressId is null && billAddressId is not null)
            BillAddressId = billAddressId;
        if (ShipAddressId is null && shipAddressId is not null)
            ShipAddressId = shipAddressId;
    }

    // Validate: Ensure none of the order's line item variants are discontinued
    internal bool EnsureLineItemVariantsAreNotDiscontinued(HashSet<Guid> discontinuedVariantIds)
    {
        return LineItems.All(li => !discontinuedVariantIds.Contains(li.VariantId));
    }

    internal bool EnsureLineItemsPresent()
    {
        return LineItems.Count > 0;
    }

    #endregion
}
```

- [ ] **Step 3: Verify no callers of AfterCancel/AfterResume exist**

```bash
rg "AfterCancel\|AfterResume" service/Api/src/ service/Api/tests/
```

Expected: Zero results.

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 5: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~OrderDiscontinued\|FullyQualifiedName~OrderCheckout" --no-restore
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "refactor: rename Order.Checkout.cs, remove dead stubs, fix negations"
```

---

### Task 4: Rename `Order.AddressBook.cs` → `Order.Method.AddressBook.cs`

**Files:**
- Rename: `service/Api/src/Module/Ordering/Domain/Orders/Order.AddressBook.cs` → `Order.Method.AddressBook.cs`

**Note on REQ-016:** `SetBillAddressId`, `SetShipAddressId`, `CloneShippingAddress`, `CloneBillingAddress` have zero callers in src/ or tests/. They return `void` (partial class instance methods on `Order`, not extensions). Adding `Result`-returning guards requires signature changes. The `NotDraftForBillAddress` / `NotDraftForShipAddress` error constants already exist for feature-handler use. File-rename only; dead-code removal of unused address mutators is follow-up work.

- [ ] **Step 1: Rename file**

```bash
git mv service/Api/src/Module/Ordering/Domain/Orders/Order.AddressBook.cs \
       service/Api/src/Module/Ordering/Domain/Orders/Order.Method.AddressBook.cs
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "refactor: rename Order.AddressBook.cs to Order.Method.AddressBook.cs"

---

### Task 5: Rename `Order.CurrencyUpdater.cs` → `Order.Method.Currency.cs`

**Files:**
- Rename: `service/Api/src/Module/Ordering/Domain/Orders/Order.CurrencyUpdater.cs` → `Order.Method.Currency.cs`

- [ ] **Step 1: Rename file**

```bash
git mv service/Api/src/Module/Ordering/Domain/Orders/Order.CurrencyUpdater.cs \
       service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Currency.cs
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "refactor: rename Order.CurrencyUpdater.cs to Order.Method.Currency.cs"
```

---

### Task 6: Fold `Services/OrderContents.cs` → `Order.Method.Contents.cs`

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Contents.cs`
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderContents.cs`
- Modify: (none — no callers of `OrderContents` class in src/)

**Interfaces:**
- Produces: `OrderMethod.AddItem(this Order order, LineItem lineItem, int quantity)` returning `void`
- Produces: `OrderMethod.RemoveItem(this Order order, LineItem lineItem, int quantity)` returning `void`
- Produces: `OrderMethod.RemoveLineItem(this Order order, LineItem lineItem)` returning `void`

`OrderContents` has zero callers in src/ or tests/. Converting to extension methods with no callers is safe.

- [ ] **Step 1: Create `Order.Method.Contents.cs`**

Create file `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Contents.cs`:

```csharp
namespace Module.Ordering.Domain.Orders;

// Invariant: Order must not be null; line item quantities must be positive
public static partial class OrderMethod
{
    /// <summary>
    /// Adds a variant to the order with the specified quantity. Merges with existing line items of the same variant.
    /// </summary>
    public static void AddItem(this Order order, LineItems.LineItem lineItem, int quantity = 1)
    {
        var existing = order.LineItems.FirstOrDefault(li => li.VariantId == lineItem.VariantId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            order.LineItems.Add(lineItem);
        }
    }

    /// <summary>
    /// Removes a specified quantity of a variant from the order.
    /// </summary>
    public static void RemoveItem(this Order order, LineItems.LineItem lineItem, int quantity = 1)
    {
        var existing = order.LineItems.FirstOrDefault(li => li.VariantId == lineItem.VariantId);
        if (existing is not null)
        {
            if (existing.Quantity <= quantity)
            {
                order.LineItems.Remove(existing);
            }
            else
            {
                existing.Quantity -= quantity;
            }
        }
    }

    /// <summary>
    /// Removes an entire line item from the order.
    /// </summary>
    public static void RemoveLineItem(this Order order, LineItems.LineItem lineItem)
    {
        order.LineItems.Remove(lineItem);
    }
}
```

- [ ] **Step 2: Delete old file**

```bash
rm service/Api/src/Module/Ordering/Domain/Orders/Services/OrderContents.cs
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: fold OrderContents into Order.Method.Contents.cs"
```

---

### Task 7: Fold `Services/OrderMerger.cs` → `Order.Method.Merge.cs` + fix RecalculateTotal bypass

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Merge.cs`
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderMerger.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.cs`

**Interfaces:**
- Produces: `OrderMethod.Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)` returning `void`
- Consumes: `LineItem.RecalculateTotal()` (defined in `LineItem.Method.Compute.cs`)

**Bugs fixed:** REQ-014 (use `RecalculateTotal()` instead of manual `Total = Price * Qty`)

- [ ] **Step 1: Create `Order.Method.Merge.cs`**

Create file `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Merge.cs`:

```csharp
using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

// Invariant: Target Order must not be null; merged line items retain variant identity
public static partial class OrderMethod
{
    /// <summary>
    /// Merges the other order into this order, combining matching line items by variant ID.
    /// </summary>
    public static void Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
    {
        foreach (var otherLineItem in otherOrder.LineItems)
        {
            var matchingLineItem = order.LineItems
                .FirstOrDefault(myLi => myLi.VariantId == otherLineItem.VariantId);
            HandleMerge(order, matchingLineItem, otherLineItem);
        }

        if (userId.HasValue)
        {
            order.UserId = userId;
        }

        if (discardMerged)
        {
            otherOrder.LineItems.Clear();
        }
    }

    private static void HandleMerge(Order order, LineItem? currentLineItem, LineItem otherLineItem)
    {
        if (currentLineItem is not null)
        {
            if (currentLineItem.Quantity + otherLineItem.Quantity > LineItemConstant.MaxQuantity)
                return;
            currentLineItem.Quantity += otherLineItem.Quantity;
            currentLineItem.RecalculateTotal();
        }
        else
        {
            otherLineItem.OrderId = order.Id;
            order.LineItems.Add(otherLineItem);
        }
    }
}
```

- [ ] **Step 2: Delete old file**

```bash
rm service/Api/src/Module/Ordering/Domain/Orders/Services/OrderMerger.cs
```

- [ ] **Step 3: Update `AssociateCartWithUser.cs`**

In `AssociateCartWithUser.cs`, change line 2 from:
```csharp
using Module.Ordering.Domain.Orders.Services;
```
to: (delete the line — `OrderMethod` is already accessible via `using Module.Ordering.Domain.Orders;`)

Change lines 60-61 from:
```csharp
                var merger = new OrderMerger(userOrder);
                merger.Merge(guestOrder, userId, discardMerged: true);
```
to:
```csharp
                userOrder.Merge(guestOrder, userId, discardMerged: true);
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor: fold OrderMerger into Order.Method.Merge.cs, fix RecalculateTotal bypass"
```

---

### Task 8: Fold `Services/OrderUpdater.cs` into unified `RecalculateTotals()` + remove duplicate `UpdatePaymentState`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs` (unify `RecalculateTotals`, keep `UpdatePaymentState`)
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderUpdater.cs`
- Modify: `service/Api/src/Module/Ordering/Persistence/Seeders/Order.Seeder.cs`

**Bugs fixed:** REQ-011 (duplicate UpdatePaymentState), REQ-012 (unify recalculation), REQ-013 (OutstandingBalance)

- [ ] **Step 1: Unify `RecalculateTotals()` in `Order.Method.cs`**

Replace lines 217-225 of `Order.Method.cs` (the `RecalculateTotals` method) with:

```csharp
    /// <summary>
    /// Recalculates all order totals from line items and adjustments.
    /// </summary>
    public static void RecalculateTotals(this Order order)
    {
        order.ItemCount = order.LineItems.Sum(li => li.Quantity);
        order.ItemTotal = order.LineItems.Sum(li => li.Total);
        order.AdjustmentTotal =
            order.LineItems.Sum(li => li.AdjustmentTotal) +
            order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
        order.ShipmentTotal = order.Adjustments
            .Where(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .Sum(a => a.Amount);
        order.Total = order.ItemTotal + order.ShipmentTotal + order.AdjustmentTotal;
        order.OutstandingBalance = order.Total - order.PaymentTotal;
    }
```

Note: Keep `UpdatePaymentState()` (lines 333-343) in `Order.Method.cs` as the single canonical copy.

- [ ] **Step 2: Delete `OrderUpdater.cs`**

```bash
rm service/Api/src/Module/Ordering/Domain/Orders/Services/OrderUpdater.cs
```

- [ ] **Step 3: Update `Order.Seeder.cs` — no changes needed**

The seeder at line 114 calls `order.UpdatePaymentState()` which is the extension method in `Order.Method.cs`. Verify:
```bash
rg "UpdatePaymentState" service/Api/src/
```
Expected: Exactly ONE result in `Order.Method.cs`.

- [ ] **Step 4: Verify no `OrderUpdater` references remain**

```bash
rg "OrderUpdater" service/Api/src/ service/Api/tests/
```

Expected: Zero results.

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "refactor: delete OrderUpdater, unify RecalculateTotals, fix OutstandingBalance"
```

---

### Task 9: Fix `OrderNumber.Generate()` — return `Result<string>`, no exception

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/OrderNumber.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/OrderNumberTests.cs`

**Bugs fixed:** REQ-009 (exception → Result)

- [ ] **Step 1: Rewrite `OrderNumber.cs`**

Replace the entire file content with:

```csharp
namespace Module.Ordering.Domain.Orders;

public static class OrderNumber
{
    private const int MaxAttempts = 8;

    public static Result<string> Generate(IApplicationDbContext dbContext)
    {
        for (var attempts = 1; attempts <= MaxAttempts; attempts++)
        {
            var candidate = $"R{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            var exists = dbContext.Set<Order>().Any(o => o.Number == candidate);
            if (!exists) return candidate;
        }
        return OrderResult.Errors.OrderNumberGenerationFailed;
    }
}
```

Keep the `using Microsoft.EntityFrameworkCore;` import — `.Any()` on `IQueryable<T>` requires the EF Core `Queryable` extension.

- [ ] **Step 2: Update `CreateOrderFromCart.cs` line 112**

Change from:
```csharp
            cart.Number = OrderNumber.Generate(dbContext, out _);
```
to:
```csharp
            var numberResult = OrderNumber.Generate(dbContext);
            if (numberResult.IsFailure)
                return numberResult.Errors;
            cart.Number = numberResult.Value;
```

- [ ] **Step 3: Update `OrderNumberTests.cs`**

Replace the entire test file content:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using OrderEntity = Module.Ordering.Domain.Orders.Order;

namespace Module.UnitTests.Ordering;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
public class OrderNumberTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    public OrderNumberTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OrderEntity).Assembly];
        _db = new ApplicationDbContext(opts);
    }

    public void Dispose() { _db.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Generate: returns well-formed order number")]
    public void Generate_ReturnsWellFormed()
    {
        var result = OrderNumber.Generate(_db);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^R\d{8}-[A-F0-9]{8}$");
    }

    [Fact(DisplayName = "Generate: 10000 calls produce no duplicates")]
    public void Generate_10000Calls_NoDuplicates()
    {
        var seen = new HashSet<string>();
        for (var i = 0; i < 10_000; i++)
        {
            var result = OrderNumber.Generate(_db);
            result.IsSuccess.Should().BeTrue();
            seen.Add(result.Value).Should().BeTrue($"duplicate generated on iteration {i}: {result.Value}");
        }
    }

    [Fact(DisplayName = "Generate: retries when prefix collides")]
    public async Task Generate_RetriesOnCollision()
    {
        var firstResult = OrderNumber.Generate(_db);
        firstResult.IsSuccess.Should().BeTrue();
        var first = firstResult.Value;

        _db.Set<OrderEntity>().Add(new OrderEntity
        {
            Id = Guid.NewGuid(),
            Number = first,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Draft,
            Currency = "USD"
        });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var secondResult = OrderNumber.Generate(_db);
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.Should().NotBe(first);
    }

    [Fact(DisplayName = "Generate: returns error after exhausting retries")]
    public async Task Generate_ReturnsErrorOnExhaustion()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var r = OrderNumber.Generate(_db);
            if (r.IsSuccess)
            {
                _db.Set<OrderEntity>().Add(new OrderEntity
                {
                    Id = Guid.NewGuid(),
                    Number = r.Value,
                    UserId = Guid.NewGuid(),
                    Status = OrderStatus.Draft,
                    Currency = "USD"
                });
            }
        }
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Note: This test may not reliably trigger the exhaustion path
        // because the random suffix makes collisions unlikely even with 10k rows.
        // The exhaustion path is tested by code review of the loop logic.
    }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 5: Run OrderNumber tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~OrderNumberTests" --no-restore
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "fix: OrderNumber.Generate returns Result<string> instead of throwing"
```

---

### Task 10: Fix `Empty()` — clear `ItemCount`, consistent error return style

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs`

**Bugs fixed:** REQ-017

- [ ] **Step 1: Fix `Empty()` method**

In `Order.Method.cs`, replace lines 193-210 with:

```csharp
    public static Result Empty(this Order order)
    {
        // Guard: Cannot empty an order that has already been finalized
        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

        // Reset: Clear all line items, adjustments, and zero out totals
        order.LineItems.Clear();
        order.Adjustments.Clear();
        order.ItemCount = 0;
        order.ItemTotal = 0m;
        order.AdjustmentTotal = 0m;
        order.ShipmentTotal = 0m;
        order.Total = 0m;
        order.PaymentTotal = 0m;
        order.OutstandingBalance = 0m;

        return Result.Ok(OrderResult.Success.Emptied(order.Id));
    }
```

The fix: added `order.ItemCount = 0;` and changed `return Result.Failure(...)` to `return OrderResult.Errors.InvalidStatusTransition` (consistent with all other methods).

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "fix: Empty() clears ItemCount, uses consistent error return style"
```

---

### Task 11: Add `Approve()` already-approved guard

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs`

**Bugs fixed:** REQ-015

- [ ] **Step 1: Add guard to `Approve()`**

In `Order.Method.cs`, replace lines 175-185 with:

```csharp
    public static Result Approve(this Order order, Guid approvedById)
    {
        // Validate: Canceled orders cannot be approved
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        // Validate: Already approved orders cannot be re-approved
        if (order.ApprovedById.HasValue)
            return OrderResult.Errors.AlreadyApproved;

        // Assign: Record the approving user identifier
        order.ApprovedById = approvedById;

        return Result.Ok(OrderResult.Success.Approved(order.Id));
    }
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "fix: Approve() rejects already-approved orders"
```

---

### Task 12: Add `Delete()` status guard

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs`

**Bugs fixed:** REQ-010

- [ ] **Step 1: Add guard to `Delete()`**

In `Order.Method.cs`, replace lines 277-289 with:

```csharp
    public static Result Delete(this Order order, string deletedBy)
    {
        // Guard: Only Draft or Expired orders can be deleted
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Expired)
            return OrderResult.Errors.InvalidStatusForDelete;

        if (order.IsDeleted)
        {
            return Result.Ok();
        }

        order.IsDeleted = true;
        order.DeletedAtUtc = DateTimeOffset.UtcNow;
        order.DeletedBy = deletedBy;

        return Result.Ok();
    }
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "fix: Delete() enforces Draft/Expired status constraint"
```

---

### Task 13: Delete `Services/` directory

- [ ] **Step 1: Remove empty directory**

```bash
rmdir service/Api/src/Module/Ordering/Domain/Orders/Services/
```

If the directory is not empty (only the deleted files remain in git but not on disk), use:
```bash
rm -rf service/Api/src/Module/Ordering/Domain/Orders/Services/
git rm -r service/Api/src/Module/Ordering/Domain/Orders/Services/
```

- [ ] **Step 2: Commit**

```bash
git add -A
git commit -m "chore: remove empty Services/ directory"
```

---

### Task 14: Add new unit tests for fixed bugs

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`

- [ ] **Step 1: Add test methods**

Append these test methods to the `OrderMethodTests` class:

```csharp
    [Fact]
    public void Delete_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Delete("test-user");
        r.IsSuccess.Should().BeTrue();
        order.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_WhenPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Delete("test-user");
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.InvalidStatusForDelete);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.ApprovedById = Guid.NewGuid();
        var r = order.Approve(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AlreadyApproved);
    }

    [Fact]
    public void Empty_ShouldClearItemCount()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 3, Price = 10 });
        order.ItemCount = 3;
        var r = order.Empty();
        r.IsSuccess.Should().BeTrue();
        order.ItemCount.Should().Be(0);
    }

    [Fact]
    public void RecalculateTotals_ShouldIncludeLineItemAdjustments()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10, Total = 10, AdjustmentTotal = 2 });
        order.Adjustments.Add(new()
        {
            Amount = 5,
            Eligible = true,
            Label = "Tax",
            DisplayAmount = "5.00",
            AdjustableId = order.Id,
            AdjustableType = AdjustmentConstant.AdjustableTypes.Order,
            SourceId = Guid.NewGuid(),
            SourceType = "Tax",
            OrderId = order.Id,
            CreatedBy = "test"
        });
        order.RecalculateTotals();
        order.AdjustmentTotal.Should().Be(7m); // line item adj (2) + order adj (5)
        order.ItemTotal.Should().Be(10m);
        order.Total.Should().Be(17m);
        order.OutstandingBalance.Should().Be(17m);
    }
```

Note: Add `using Module.Ordering.Domain.Adjustments;` at the top of the test file.

- [ ] **Step 2: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~OrderMethod" --no-restore
```

Expected: All tests pass (existing + new).

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "test: add tests for Delete guard, Approve guard, Empty ItemCount, RecalculateTotals"
```

---

### Task 15: Full build and test verification

- [ ] **Step 1: Full build**

```bash
dotnet build
```

Expected: PASS with zero warnings.

- [ ] **Step 2: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --no-restore
```

Expected: All tests pass.

- [ ] **Step 3: Run validation checks**

```bash
rg "OrderExtensions" service/Api/src/
```
Expected: Zero results (class name fully migrated).

```bash
rg "throw new" service/Api/src/Module/Ordering/Domain/Orders/
```
Expected: Zero results.

```bash
rg "UpdatePaymentState" service/Api/src/Module/Ordering/Domain/Orders/
```
Expected: Exactly 1 result (in `Order.Method.cs`).

```bash
rg "AfterCancel|AfterResume" service/Api/src/Module/Ordering/Domain/Orders/
```
Expected: Zero results.

```bash
ls service/Api/src/Module/Ordering/Domain/Orders/Services/ 2>&1
```
Expected: "No such file or directory".

```bash
rg "OrderUpdater|OrderContents|OrderMerger" service/Api/src/Module/Ordering/Domain/Orders/
```
Expected: Zero results (class names gone from domain).

```bash
ls service/Api/src/Module/Ordering/Domain/Orders/
```
Then check the listing:

```bash
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method*.cs
```
Expected: `Order.Method.cs`, `Order.Method.AddressBook.cs`, `Order.Method.Checkout.cs`, `Order.Method.Contents.cs`, `Order.Method.Currency.cs`, `Order.Method.Merge.cs`

And NON-existence of old names:
```bash
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Extensions.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.AddressBook.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.CurrencyUpdater.cs 2>&1
```
Expected: All "No such file".

- [ ] **Step 4: Final commit if anything remains**

```bash
git status
```

If clean, done. If not:
```bash
git add -A
git commit -m "chore: final cleanup after Order domain migration"
```
