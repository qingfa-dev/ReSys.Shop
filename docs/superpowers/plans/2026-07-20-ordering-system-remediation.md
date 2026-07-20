# Ordering System Defect Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 15 defects in the Ordering module: 2 bugs (shipping double-count, Draft orders approving), 8 risks, 5 nits — per spec `spec-design-ordering-system-remediation.md`.

**Architecture:** Domain methods return `Result<T>` or `Result`. Feature handlers compose domain calls + MediatR. All changes are surgical edits in existing files; no new feature files. Tests are MSTest + FluentAssertions with in-memory EF Core.

**Tech Stack:** .NET 10, C# preview, MSTest, FluentAssertions, Moq, EF Core InMemory

## Global Constraints

- `Result` objects, not exceptions — domain methods return `Result<T>`/`Result`
- Modules never cross-reference domain types — `Module.Ordering` cannot `using Module.Inventory.Domain.*`
- Warnings-as-errors — `TreatWarningsAsErrors=true`
- All domain operations are `static` extension methods on `Order`/`LineItem`/`Adjustment`
- Existing tests in `service/Api/tests/Module.UnitTests/Ordering/` must continue passing
- Test framework: MSTest; test project uses `UseInMemoryDatabase` for unit tests; `AdditionalConfigurationsAssemblies` for EF model discovery

---

### Task 1: RM-001 — Fix RecalculateTotals shipping double-count

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs:21-23`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` (add test)

**Interfaces:**
- Consumes: `AdjustmentConstant.SourceTypes.Shipping` (string constant `"Shipping"`)
- Produces: `order.RecalculateTotals()` no longer double-counts shipping in `Total`

- [ ] **Step 1: Write the failing test**

Add to `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` after line 195 (after `RecalculateTotals_ShouldIncludeLineItemAdjustments`):

```csharp
[Fact(DisplayName = "RecalculateTotals: Total does not count shipping twice")]
public void RecalculateTotals_WithShippingAdjustment_DoesNotCountShippingTwice()
{
    var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
    order.LineItems.Add(new() { Quantity = 1, Price = 100, Total = 100 });
    order.Adjustments.Add(new Adjustment
    {
        Amount = 10,
        Eligible = true,
        Label = "Shipping",
        DisplayAmount = "10.00",
        AdjustableId = order.Id,
        AdjustableType = AdjustmentConstant.AdjustableTypes.Order,
        SourceId = Guid.NewGuid(),
        SourceType = AdjustmentConstant.SourceTypes.Shipping,
        OrderId = order.Id,
        CreatedBy = "test"
    });
    order.Adjustments.Add(new Adjustment
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

    var result = order.RecalculateTotals();

    result.IsSuccess.Should().BeTrue();
    order.ItemTotal.Should().Be(100m);
    order.ShipmentTotal.Should().Be(10m);
    order.AdjustmentTotal.Should().Be(5m);
    order.Total.Should().Be(115m); // 100 + 10 + 5, not 125
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~RecalculateTotals_WithShippingAdjustment_DoesNotCountShippingTwice"
```
Expected: FAIL. `AdjustmentTotal` is 15 (10+5), `Total` is 125.

- [ ] **Step 3: Fix the implementation**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`, change line 23 from:

```csharp
order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
```
to:
```csharp
order.Adjustments.Where(a => a.Eligible && a.SourceType != AdjustmentConstant.SourceTypes.Shipping).Sum(a => a.Amount);
```

The full block (lines 21-23) becomes:

```csharp
order.AdjustmentTotal =
    order.LineItems.Sum(li => li.AdjustmentTotal) +
    order.Adjustments.Where(a => a.Eligible && a.SourceType != AdjustmentConstant.SourceTypes.Shipping).Sum(a => a.Amount);
```

- [ ] **Step 4: Run existing tests + new test to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"
```
Expected: All pass. The `RecalculateTotals_ShouldIncludeLineItemAdjustments` test uses `SourceType = "Tax"` (not Shipping) so it still passes with `AdjustmentTotal = 7`, `Total = 17`.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs
git commit -m "fix(ordering): prevent double-counting shipping in RecalculateTotals

Exclude SourceType=Shipping adjustments from AdjustmentTotal since
ShipmentTotal already captures them. Total = ItemTotal + ShipmentTotal
+ AdjustmentTotal no longer counts shipping twice."
```

---

### Task 2: RM-002 — Fix Approve() missing Placed guard

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs:74-89`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` (add test, fix existing)

**Interfaces:**
- Consumes: `OrderResult.Errors.InvalidStatusTransition`
- Produces: `order.Approve(Guid)` rejects Draft/Expired/Canceled orders

- [ ] **Step 1: Write the failing test for Draft approval**

Add to `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` after line 158 (after `Approve_WhenAlreadyApproved_ShouldFail`):

```csharp
[Fact(DisplayName = "Approve: Draft order returns InvalidStatusTransition")]
public void Approve_DraftOrder_ShouldFail()
{
    var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
    var r = order.Approve(Guid.NewGuid());
    r.IsFailure.Should().BeTrue();
    r.Errors[0].Should().Be(OrderResult.Errors.InvalidStatusTransition);
}

[Fact(DisplayName = "Approve: Placed order succeeds")]
public void Approve_PlacedOrder_ShouldSucceed()
{
    var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
    order.LineItems.Add(new() { Quantity = 1, Price = 10 });
    order.Finalize();
    var approverId = Guid.NewGuid();
    var r = order.Approve(approverId);
    r.IsSuccess.Should().BeTrue();
    order.ApprovedById.Should().Be(approverId);
    order.ApprovedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
}
```

- [ ] **Step 2: Fix the existing `Approve_WhenAlreadyApproved_ShouldFail` test**

The existing test at line 150-158 calls `Approve()` on a Draft order with `ApprovedById` set — after the fix this will fail with `InvalidStatusTransition` before reaching `AlreadyApproved`. Change lines 150-158 of `Order.Method.Tests.cs` to:

```csharp
[Fact]
public void Approve_WhenAlreadyApproved_ShouldFail()
{
    var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
    order.LineItems.Add(new() { Quantity = 1, Price = 10 });
    order.Finalize();                          // <-- add this line
    order.ApprovedById = Guid.NewGuid();
    var r = order.Approve(Guid.NewGuid());
    r.IsFailure.Should().BeTrue();
    r.Errors[0].Should().Be(OrderResult.Errors.AlreadyApproved);
}
```

- [ ] **Step 3: Run tests to verify failures**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering.Domain.Orders.OrderMethodTests"
```
Expected: `Approve_DraftOrder_ShouldFail` FAILS (approval succeeds on Draft). `Approve_WhenAlreadyApproved_ShouldFail` FAILS (wrong error code after Draft rejection).

- [ ] **Step 4: Add the Placed guard to Approve()**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`, insert after line 76 (after the `// Guard: Reject approval if order is canceled or already approved` comment):

```csharp
if (order.Status != OrderStatus.Placed)
    return OrderResult.Errors.InvalidStatusTransition;
```

The full method becomes (lines 74-89):

```csharp
// Enforce: Only non-canceled orders can be approved; approval is one-shot
public static Result Approve(this Order order, Guid approvedById)
{
    // Guard: Reject approval if order is canceled or already approved
    if (order.Status == OrderStatus.Canceled)
        return OrderResult.Errors.AlreadyCanceled;

    if (order.Status != OrderStatus.Placed)
        return OrderResult.Errors.InvalidStatusTransition;

    if (order.ApprovedById.HasValue)
        return OrderResult.Errors.AlreadyApproved;

    // Assign: Record approver identity and timestamps for audit trail
    order.ApprovedById = approvedById;
    order.ApprovedAtUtc = DateTimeOffset.UtcNow;
    order.ModifiedAtUtc = DateTimeOffset.UtcNow;

    return Result.Ok(OrderResult.Success.Approved(order.Id));
}
```

- [ ] **Step 5: Run tests to verify pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"
```
Expected: All pass. `Approve_DraftOrder_ShouldFail` now passes. `Approve_PlacedOrder_ShouldSucceed` passes. `Approve_WhenAlreadyApproved_ShouldFail` passes with Placed context.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs
git commit -m "fix(ordering): reject Approve() on non-Placed orders

Guard order.Status != Placed before approving. Draft orders can no longer
be approved. Updated existing AlreadyApproved test to use Placed order."
```

---

### Task 3: RM-011 — Guard Order.Empty() against Expired status

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs:93-99`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` (add test)

**Interfaces:**
- Consumes: `OrderResult.Errors.InvalidStatusTransition`
- Produces: `order.Empty()` rejects Expired orders

- [ ] **Step 1: Write the failing test**

Add to `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` after line 128 (after `Empty_WhenCanceled_ShouldFail`):

```csharp
[Fact(DisplayName = "Empty: Expired order returns InvalidStatusTransition")]
public void Empty_WhenExpired_ShouldFail()
{
    var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
    order.Status = OrderStatus.Expired;
    var r = order.Empty();
    r.IsFailure.Should().BeTrue();
    r.Errors[0].Should().Be(OrderResult.Errors.InvalidStatusTransition);
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Empty_WhenExpired"
```
Expected: FAIL. `Empty()` succeeds on Expired order.

- [ ] **Step 3: Add Expired guard to Empty()**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`, change lines 94-96 from:

```csharp
// Guard: Placed orders are immutable — reject empty operation
if (order.Status == OrderStatus.Placed)
    return OrderResult.Errors.InvalidStatusTransition;
```
to:
```csharp
// Guard: Placed and Expired orders are immutable — reject empty operation
if (order.Status == OrderStatus.Placed || order.Status == OrderStatus.Expired)
    return OrderResult.Errors.InvalidStatusTransition;
```

- [ ] **Step 4: Run tests to verify pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"
```
Expected: All pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs
git commit -m "fix(ordering): reject Empty() on Expired orders"
```

---

### Task 4: RM-012 — Fix SetShippingMethod error constant

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` (add error constant)
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs:198` (use new constant)
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` (update assertion)

**Interfaces:**
- Consumes: existing `Error.Validation` factory pattern
- Produces: `OrderResult.Errors.NotDraftForShippingMethod`

- [ ] **Step 1: Add the new error constant**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs`, add after line 129 (after `NotDraftForShipAddress` definition):

```csharp
/// <summary>Only draft orders can have shipping method modified.</summary>
public static Error NotDraftForShippingMethod => Error.Validation(
    code: "Order.ShippingMethod.Update.NotDraft",
    message: "Only draft orders can have shipping method modified.");
```

- [ ] **Step 2: Fix the existing test assertion**

In `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`, line 321, change:

```csharp
r.Errors[0].Should().Be(OrderResult.Errors.NotDraftForShipAddress);
```
to:
```csharp
r.Errors[0].Should().Be(OrderResult.Errors.NotDraftForShippingMethod);
```

- [ ] **Step 3: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~SetShippingMethod_WhenPlaced"
```
Expected: FAIL. Error constant doesn't match (`NotDraftForShipAddress` != `NotDraftForShippingMethod`).

- [ ] **Step 4: Fix the implementation**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`, line 198, change:

```csharp
return OrderResult.Errors.NotDraftForShipAddress;
```
to:
```csharp
return OrderResult.Errors.NotDraftForShippingMethod;
```

- [ ] **Step 5: Run tests to verify pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"
```
Expected: All pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs
git commit -m "fix(ordering): use NotDraftForShippingMethod error in SetShippingMethod

Previously returned NotDraftForShipAddress which was semantically wrong."
```

---

### Task 5: RM-015 — Delete LineItem.FinalAmount()

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs` (delete method)
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/LineItems/LineItem.Method.Tests.cs` (remove test)

**Interfaces:**
- Consumes: none (no non-test callers)
- Produces: `FinalAmount()` no longer exists

- [ ] **Step 1: Remove the test**

In `service/Api/tests/Module.UnitTests/Ordering/Domain/LineItems/LineItem.Method.Tests.cs`, delete lines 70-80 (the entire `FinalAmount_ShouldReturnTotal` test).

- [ ] **Step 2: Remove the method**

In `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs`, delete lines 15-19 (the entire `FinalAmount()` method and its comment). The file should end with line 13:

```csharp
return Result.Ok(LineItemResult.Success.Recalculated(lineItem.Id));
```
followed by the closing bracket of `RecalculateTotal` and `#endregion` and the closing braces.

- [ ] **Step 3: Verify build and tests**

```bash
dotnet build service/Api/src/Module/Ordering/
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~LineItem"
```
Expected: Build passes, all LineItem tests pass, `FinalAmount` test no longer exists.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs service/Api/tests/Module.UnitTests/Ordering/Domain/LineItems/LineItem.Method.Tests.cs
git commit -m "refactor(ordering): remove LineItem.FinalAmount() dead abstraction

Method was a pure accessor returning Total unchanged. No non-test callers."
```

---

### Task 6: RM-003 — Remove TotalAvailable from CheckStockAvailability.Response

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Response.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Query.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs` (remove TotalAvailable from mock setups)
- Check: `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Tests.cs` (update if exists)

**Interfaces:**
- Consumes: existing `CheckStockAvailability.Response` record
- Produces: `Response` has only `VariantId` and `IsAvailable` properties

- [ ] **Step 1: Remove TotalAvailable from the Response record**

In `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Response.cs`, change lines 5-10 from:

```csharp
public sealed record Response
{
    public Guid VariantId { get; init; }
    public bool IsAvailable { get; init; }
    public int TotalAvailable { get; init; }
}
```
to:
```csharp
public sealed record Response
{
    public Guid VariantId { get; init; }
    public bool IsAvailable { get; init; }
}
```

- [ ] **Step 2: Remove TotalAvailable from the Query handler response construction**

In `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Query.cs`, change lines 17-18 and 23-28:

Change line 17-18 from:
```csharp
if (req.Quantity <= 0)
    return new Response { VariantId = req.VariantId, IsAvailable = true, TotalAvailable = 0 };
```
to:
```csharp
if (req.Quantity <= 0)
    return new Response { VariantId = req.VariantId, IsAvailable = true };
```

Change lines 23-28 from:
```csharp
return new Response
{
    VariantId = req.VariantId,
    IsAvailable = isAvailable,
    TotalAvailable = 0
};
```
to:
```csharp
return new Response
{
    VariantId = req.VariantId,
    IsAvailable = isAvailable
};
```

- [ ] **Step 3: Remove TotalAvailable from UpdateCartItemQuantity tests mock setups**

In `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs`:

Line 83 — change from:
```csharp
new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = true, TotalAvailable = 10 }));
```
to:
```csharp
new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = true }));
```

Line 126 — change from:
```csharp
new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = false, TotalAvailable = 3 }));
```
to:
```csharp
new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = false }));
```

- [ ] **Step 4: Find and fix any other references**

```bash
grep -r "TotalAvailable" service/Api/src/ service/Api/tests/ --include="*.cs"
```
Expected: zero results outside of the files already changed in Steps 1-3. If any other files reference `TotalAvailable`, update them similarly.

- [ ] **Step 5: Run Inventory tests to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CheckStockAvailability"
```
Expected: All pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Response.cs service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/CheckStockAvailability.Query.cs service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs
git commit -m "refactor(inventory): remove TotalAvailable from CheckStockAvailability.Response

Field was always set to 0 — dead data. Query is lightweight UX pre-validation."
```

---

### Task 7: RM-004 + RM-005 — Fix UpdateCartItemQuantity dead code + boundary violation

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`

**Interfaces:**
- Consumes: `CheckStockAvailability.Response` (now without `TotalAvailable`)
- Produces: handler without dead `IsFailure` check, without `using Inventory.Domain.*`

- [ ] **Step 1: Remove the cross-module import and dead code**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`:

**Line 1** — delete:
```csharp
using Module.Inventory.Domain.StockLocations.StockItems;
```

**Lines 55-56** — delete the dead `IsFailure` check:
```csharp
if (stockResult.IsFailure)
    return stockResult.Errors;
```

**Line 58-59** — keep the `IsAvailable` check but change the error from `StockItemResult.Errors.InsufficientStock` to `OrderResult.Errors.QuantityNotPositive`:

```csharp
if (!stockResult.Value.IsAvailable)
    return OrderResult.Errors.CartQuantityInvalid;
```

The full relevant section (lines 46-60 after edits) becomes:

```csharp
// Validate: Stock availability via Inventory module's reservation-aware query.
var stockResult = await sender.Send(
    new CheckStockAvailability.Query(new CheckStockAvailability.Request
    {
        VariantId = lineItem.VariantId,
        Quantity = command.Request.Quantity
    }),
    cancellationToken);

if (!stockResult.Value.IsAvailable)
    return OrderResult.Errors.CartQuantityInvalid;
```

- [ ] **Step 2: Run tests to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~UpdateCartItemQuantity"
```
Expected: `Handle_ShouldFail_WhenInsufficientStock` still passes (error type changed from `InsufficientStock` to `CartQuantityInvalid`). Test assertion at line 134 (`result.IsFailure.Should().BeTrue()`) doesn't check the specific error code.

- [ ] **Step 3: Verify no Inventory domain references remain in Ordering**

```bash
grep -r "Module.Inventory.Domain" service/Api/src/Module/Ordering/ --include="*.cs"
```
Expected: zero results.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs
git commit -m "fix(ordering): remove dead IsFailure check and Inventory domain import

CheckStockAvailability handler never returns failures. Replaced cross-module
StockItemResult.Errors.InsufficientStock with OrderResult.Errors.CartQuantityInvalid."
```

---

### Task 8: RM-014 — Clean up dead test setup in UpdateCartItemQuantity tests

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs`

**Interfaces:**
- Produces: tests without StockLocation seeding, StockItem assembly reference, or Inventory domain imports

- [ ] **Step 1: Remove dead imports and assembly reference**

In `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs`:

**Line 1** — delete:
```csharp
using Module.Inventory.Domain.StockLocations.StockItems;
```

**Line 2** — delete:
```csharp
using Module.Inventory.Domain.StockLocations;
```

**Line 31** — remove `typeof(StockItem).Assembly` from the array. Change lines 29-31 from:

```csharp
ApplicationDbContext.AdditionalConfigurationsAssemblies = [
    typeof(Order).Assembly,
    typeof(StockItem).Assembly
];
```
to:
```csharp
ApplicationDbContext.AdditionalConfigurationsAssemblies = [
    typeof(Order).Assembly
];
```

- [ ] **Step 2: Remove StockLocation seeding from both test methods**

**Lines 59-61** — delete the StockLocation seed block from `Handle_ShouldUpdateQuantity_WhenItemExists`:

```csharp
var location = StockLocationMethod.Create("Main").Value;
_dbContext.Set<StockLocation>().Add(location);
await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
```

**Lines 102-104** — delete the same StockLocation seed block from `Handle_ShouldFail_WhenInsufficientStock`:

```csharp
var location = StockLocationMethod.Create("Main").Value;
_dbContext.Set<StockLocation>().Add(location);
await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
```

- [ ] **Step 3: Run tests to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~UpdateCartItemQuantity"
```
Expected: All 3 tests pass.

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Tests.cs
git commit -m "test(ordering): remove dead StockItem/StockLocation setup from UpdateCartItemQuantity tests

Handler uses CheckStockAvailability via ISender, not direct stock queries."
```

---

### Task 9: RM-006 + RM-007 — Fix AddToCart location check + null-forgiving

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`

**Interfaces:**
- Consumes: `StockItem.StockLocation.Active` (navigation property on `StockItem`)
- Produces: `primaryLocation` query excludes inactive locations; `currentUser.UserId!` replaced with `??`

- [ ] **Step 1: Add StockLocation.Active filter to the primary location query**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`, lines 69-72, change from:

```csharp
var primaryLocation = await dbContext.Set<StockItem>()
    .Where(si => si.VariantId == request.VariantId && si.CountOnHand > 0)
    .OrderByDescending(si => si.CountOnHand)
    .FirstOrDefaultAsync(cancellationToken);
```
to:
```csharp
var primaryLocation = await dbContext.Set<StockItem>()
    .Include(si => si.StockLocation)
    .Where(si => si.VariantId == request.VariantId && si.CountOnHand > 0 && si.StockLocation.Active)
    .OrderByDescending(si => si.CountOnHand)
    .FirstOrDefaultAsync(cancellationToken);
```

- [ ] **Step 2: Replace null-forgiving operator**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`, lines 76-78, change from:

```csharp
var cartToken = currentUser.IsAuthenticated
    ? currentUser.UserId!
    : currentUser.SessionId ?? string.Empty;
```
to:
```csharp
var cartToken = currentUser.IsAuthenticated
    ? currentUser.UserId ?? string.Empty
    : currentUser.SessionId ?? string.Empty;
```

- [ ] **Step 3: Run AddToCart tests to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~AddToCart"
```
Expected: All pass. The in-memory test DB doesn't exercise the `Include` navigation filter, but existing behavior is preserved for active locations.

- [ ] **Step 4: Verify build**

```bash
dotnet build service/Api/src/Module/Ordering/
```
Expected: Build passes with no warnings.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs
git commit -m "fix(ordering): filter inactive locations in AddToCart, replace null-forgiving op

Guard reservation against inactive StockLocations by including them in the
query filter. Replace currentUser.UserId! with ?? string.Empty for safety."
```

---

### Task 10: RM-010 — Simplify CancelOrder wasPlaced check

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs:45`

**Interfaces:**
- Consumes: `entity.Status`, `entity.CompletedAtUtc`
- Produces: `wasPlaced` uses only `Status == Placed`

- [ ] **Step 1: Simplify the definition**

In `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs`, line 45, change from:

```csharp
var wasPlaced = entity.Status == OrderStatus.Placed && entity.CompletedAtUtc.HasValue;
```
to:
```csharp
var wasPlaced = entity.Status == OrderStatus.Placed;
```

- [ ] **Step 2: Run CancelOrder tests to verify**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CancelOrder"
```
Expected: All pass.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs
git commit -m "refactor(ordering): simplify CancelOrder wasPlaced check

Both Place() and Finalize() always set CompletedAtUtc when transitioning
to Placed. The HasValue check was a redundant defensive guard."
```

---

### Task 11: RM-013 — Remove redundant catch block in CreateOrderFromCart

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs:160-163`

**Interfaces:**
- Produces: transaction auto-rollback via `await using` on exception; no explicit `catch { RollbackAsync(); throw; }`

- [ ] **Step 1: Remove the redundant catch block**

In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`, delete lines 160-163:

```csharp
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
```

The `try` block at line 92 should now close at line 158 (after `await transaction.CommitAsync(cancellationToken);`) with a single `}` for the `try`. The `await using` on the transaction at line 90 handles auto-rollback on any uncaught exception within the scope.

- [ ] **Step 2: Verify the enclosing braces and build**

The resulting structure should be:

```csharp
            try
            {
                // ... stock deduction logic ...
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return StockItemResult.Errors.ConcurrencyConflict(
                    cart.LineItems.First().VariantId);
            }

            await transaction.CommitAsync(cancellationToken);
```

Note: There is no outer `catch` block anymore. The `DbUpdateConcurrencyException` catch is inside the `try`, and if a non-concurrency exception occurs, it propagates up naturally with the `await using` ensuring rollback.

- [ ] **Step 3: Build and run checkout tests**

```bash
dotnet build service/Api/src/Module/Ordering/
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateOrderFromCart"
```
Expected: Build passes. All checkout tests pass.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git commit -m "refactor(ordering): remove redundant catch/rollback/rethrow in CreateOrderFromCart

EF Core transaction auto-rollback on DisposeAsync when not committed.
Explicit RollbackAsync before throw was redundant."
```

---

### Task 12: RM-008 + RM-009 — CartExpiryJob pagination + domain Delete()

**Files:**
- Modify: `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Backgrounds/CartExpiryJobTests.cs`

**Interfaces:**
- Consumes: `cart.Delete(string deletedBy)`, `OrderConstant.Defaults.CreatedBy`
- Produces: batch-based expiry processing; `DeletedBy` populated on expired carts

- [ ] **Step 1: Write the batching test**

Replace the entire content of `service/Api/tests/Module.UnitTests/Ordering/Backgrounds/CartExpiryJobTests.cs` with:

```csharp
using Microsoft.EntityFrameworkCore;

using Module.Ordering.Backgrounds;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Backgrounds;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Component", "CartExpiryJob")]
public class CartExpiryJobTests
{
    [Fact(DisplayName = "RunAsync: expires drafts past cutoff in batches, uses Delete() domain method")]
    public async Task RunAsync_ShouldExpireCartsInBatches()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        await using var db = new ApplicationDbContext(options);

        var oldDate = DateTimeOffset.UtcNow.AddDays(-10);
        for (var i = 0; i < 3; i++)
        {
            db.Set<Order>().Add(new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Draft,
                CreatedAtUtc = oldDate,
                ModifiedAtUtc = null,
                IsDeleted = false
            });
        }
        db.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ModifiedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await db.SaveChangesAsync();

        var job = new CartExpiryJob(db, new Mock<ILogger<CartExpiryJob>>().Object, afterDays: 1);
        await job.RunAsync();

        var expired = await db.Set<Order>().Where(o => o.Status == OrderStatus.Expired).ToListAsync();
        expired.Should().HaveCount(3);
        expired.Should().AllSatisfy(o =>
        {
            o.IsDeleted.Should().BeTrue();
            o.DeletedBy.Should().Be("System");
            o.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        });

        var recent = await db.Set<Order>().Where(o => o.Status == OrderStatus.Draft).ToListAsync();
        recent.Should().HaveCount(1);
    }

    [Fact(DisplayName = "RunAsync: processes in batches of 500")]
    public async Task RunAsync_ShouldProcessInBatches()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        await using var db = new ApplicationDbContext(options);

        var oldDate = DateTimeOffset.UtcNow.AddDays(-10);
        for (var i = 0; i < 750; i++)
        {
            db.Set<Order>().Add(new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Draft,
                CreatedAtUtc = oldDate,
                ModifiedAtUtc = null,
                IsDeleted = false
            });
        }
        await db.SaveChangesAsync();

        var job = new CartExpiryJob(db, new Mock<ILogger<CartExpiryJob>>().Object, afterDays: 1);
        await job.RunAsync();

        var expired = await db.Set<Order>().Where(o => o.Status == OrderStatus.Expired).ToListAsync();
        expired.Should().HaveCount(750);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CartExpiryJob"
```
Expected: `RunAsync_ShouldExpireCartsInBatches` FAILS because `DeletedBy` is not set (direct mutation doesn't use `Delete()`). `RunAsync_ShouldProcessInBatches` may pass if no OOM occurs but the code is not batched yet.

- [ ] **Step 3: Implement batching and domain Delete() call**

Replace the entire content of `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs` with:

```csharp
using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Backgrounds;

/// <summary>Background job that expires draft carts past a configurable inactivity cutoff.</summary>
// Contract: pre=dbContext!=null && logger!=null, post=expired carts have Status==Expired && IsDeleted==true
public sealed partial class CartExpiryJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CartExpiryJob> _logger;
    private readonly int _afterDays;

    internal const int BatchSize = 500;

    public CartExpiryJob(IApplicationDbContext dbContext, ILogger<CartExpiryJob> logger, int afterDays = 7)
    {
        _dbContext = dbContext;
        _logger = logger;
        _afterDays = afterDays;
    }

    /// <summary>Executes the expiry sweep — transitions draft carts past the cutoff to Expired with soft-delete, in batches.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunAsync(CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-_afterDays);
        var totalExpired = 0;

        List<Order> expired;
        do
        {
            expired = await _dbContext.Set<Order>()
                .Where(o => o.Status == OrderStatus.Draft
                    && (o.ModifiedAtUtc == null || o.ModifiedAtUtc < cutoff)
                    && !o.IsDeleted)
                .Take(BatchSize)
                .ToListAsync(ct);

            foreach (var cart in expired)
            {
                cart.Status = OrderStatus.Expired;
                cart.Delete(OrderConstant.Defaults.CreatedBy);
            }

            totalExpired += expired.Count;
            await _dbContext.SaveChangesAsync(ct);

            Loggers.Found(_logger, expired.Count, cutoff);
        } while (expired.Count == BatchSize);

        Loggers.Completed(_logger, totalExpired);
    }
}
```

- [ ] **Step 4: Run tests to verify pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CartExpiryJob"
```
Expected: Both tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs service/Api/tests/Module.UnitTests/Ordering/Backgrounds/CartExpiryJobTests.cs
git commit -m "fix(ordering): add pagination to CartExpiryJob, use domain Delete() method

Process expired carts in batches of 500 to prevent OOM. Use
cart.Delete(Default.CreatedBy) instead of direct mutation to preserve
DeletedBy in audit trail."
```

---

### Task 13: Final verification — full build + all Ordering tests

- [ ] **Step 1: Build the entire solution**

```bash
dotnet build
```
Expected: Build succeeds with zero warnings (warnings-as-errors).

- [ ] **Step 2: Run all Ordering unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"
```
Expected: All tests pass.

- [ ] **Step 3: Verify no cross-module boundary violations**

```bash
grep -r "Module.Inventory.Domain" service/Api/src/Module/Ordering/ --include="*.cs"
```
Expected: zero results.

- [ ] **Step 4: Commit if there are any remaining uncommitted changes**

```bash
git status
```
If clean, done. If not, commit any stragglers with an appropriate message.
