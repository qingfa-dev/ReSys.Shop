---
title: Order Domain — Result Pattern Compliance for All Methods
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
tags: design, ordering, domain, result-pattern, service/Api
---

# Introduction

The codebase's non-negotiable rule states "all domain operations return `Result<T>` or `Result`". However, 3 of the 10 surviving `OrderMethod` methods return `void`: `RecalculateTotals()`, `UpdatePaymentState()`, and `Merge()`. These methods mutate order state without signaling success or failure, breaking the Result-only contract. Additionally, `RecalculateTotals()` is the most heavily called domain method (12 call sites across 11 feature handlers), and its callers currently assume it can never fail — an assumption that should be explicit via `Result` return.

This specification defines converting the 3 void methods to return `Result`, adding missing `Success` result factories, and updating all 14 call sites in feature handlers, seeders, and unit tests.

## 1. Purpose & Scope

**Purpose**: Convert `RecalculateTotals()`, `UpdatePaymentState()`, and `Merge()` from `void` to `Result`-returning methods. Add missing `Success` factories. Update all callers to handle the Result. Update the unit test to assert the Result.

**Scope**: Files under `service/Api/src/Module/Ordering/Domain/Orders/` (3 methods), 12 feature handler files (callers of `RecalculateTotals`), 1 seeder file (caller of `UpdatePaymentState`), 1 feature file (caller of `Merge`), and 1 test file (caller of `RecalculateTotals`). No changes to non-Ordering modules.

**Audience**: Backend developers implementing the Result-pattern conversion.

**Assumptions**:
- The dead-code removal (v2.0) is complete — only 10 methods survive.
- All 3 methods are pure computations that cannot fail at the domain level (no IO, no external dependencies). They return `Result.Ok(Success.X)` unconditionally.
- The 12 `RecalculateTotals` callers are all inside feature handler `Handle()` methods that already return `Result<T>`.
- The `Finalize()` state machine method (in `StateMachine.cs`) internally calls `RecalculateTotals()` and must also be updated.

## 2. Definitions

| Term | Definition |
|------|-----------|
| **Result pattern** | Every domain operation that mutates state returns `Result` (for void-like operations) or `Result<T>` (for value-returning operations). Methods that query state without mutation (e.g., `IsPaid`, properties) are exempt. |
| **Void method** | A method returning `void` that mutates entity state without signaling completion. These violate the Result-only rule. |
| **Success factory** | A static method in `OrderResult.Success` that returns a formatted string message for a successful operation. |

## 3. Requirements, Constraints & Guidelines

### Method Signature Changes (REQ)

- **REQ-M01**: Change `RecalculateTotals(this Order order)` from `void` to `Result`.
  ```csharp
  // Before
  public static void RecalculateTotals(this Order order)

  // After
  public static Result RecalculateTotals(this Order order)
  ```
  Return value: `return Result.Ok(OrderResult.Success.Recalculated(order.Id));` as the last line.

- **REQ-M02**: Change `UpdatePaymentState(this Order order)` from `void` to `Result`.
  ```csharp
  // Before
  public static void UpdatePaymentState(this Order order)

  // After
  public static Result UpdatePaymentState(this Order order)
  ```
  Return value: `return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));` as the last line.

- **REQ-M03**: Change `Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)` from `void` to `Result`.
  ```csharp
  // Before
  public static void Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)

  // After
  public static Result Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
  ```
  Return value: `return Result.Ok(OrderResult.Success.Merged(order.Id));` as the last line.

### Result Factory Additions (REQ)

- **REQ-R01**: Add to `OrderResult.Success`:
  ```csharp
  /// <summary>Order totals were recalculated.</summary>
  public static string Recalculated(Guid id) => $"Order with ID '{id}' totals were recalculated.";
  /// <summary>Payment state was derived and updated.</summary>
  public static string PaymentStateUpdated(Guid id) => $"Order with ID '{id}' payment state was updated.";
  ```

### Caller Updates (REQ)

- **REQ-C01**: Update `Finalize()` in `Order.Method.StateMachine.cs` (line 25 — calls `order.RecalculateTotals()`). The method already returns `Result`, so capture and propagate:
  ```csharp
  // Before
  order.RecalculateTotals();

  // After
  var recalcResult = order.RecalculateTotals();
  if (recalcResult.IsFailure)
      return recalcResult.Errors;
  ```

- **REQ-C02**: Update 12 feature handler callers of `RecalculateTotals()` across these files:

  | File | Line Pattern |
  |------|-------------|
  | `Features/Storefront/Cart/AddItem/AddToCart.cs` | `cart.RecalculateTotals();` |
  | `Features/Storefront/Cart/EmptyCart/EmptyCart.cs` | `cart.RecalculateTotals();` |
  | `Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs` | `cart.RecalculateTotals();` |
  | `Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` | `cart.RecalculateTotals();` |
  | `Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` | `cart.RecalculateTotals();` |
  | `Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs` | `cart.RecalculateTotals();` |
  | `Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.cs` | `targetOrder.RecalculateTotals();` |
  | `Features/Admin/Orders/AddLineItem/AddOrderLineItem.cs` | `order.RecalculateTotals();` |
  | `Features/Admin/Orders/RemoveLineItem/RemoveOrderLineItem.cs` | `order.RecalculateTotals();` |
  | `Features/Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs` | `order.RecalculateTotals();` |
  | `Features/Admin/Orders/UpdateShippingMethod/UpdateOrderShippingMethod.cs` | `order.RecalculateTotals();` |

  Each replacement follows this pattern:
  ```csharp
  // Before
  cart.RecalculateTotals();

  // After
  var recalcResult = cart.RecalculateTotals();
  if (recalcResult.IsFailure)
      return recalcResult.Errors;
  ```
  Adapt the variable name (`order` vs `cart`) to match the existing local.

- **REQ-C03**: Update 1 caller of `UpdatePaymentState()` in `Order.Seeder.cs` (line 114). The seeder is an async void-style method — replace the bare call with an ignored result:
  ```csharp
  // Before
  order.UpdatePaymentState();

  // After
  order.UpdatePaymentState(); // Result unused — seeder writes domain state directly
  ```
  No propagation needed. The Result is intentionally discarded because the seeder writes directly to the database context.

- **REQ-C04**: Update 1 caller of `Merge()` in `AssociateCartWithUser.cs` (line 59). The handler returns `Result`:
  ```csharp
  // Before
  userOrder.Merge(guestOrder, userId, discardMerged: true);

  // After
  var mergeResult = userOrder.Merge(guestOrder, userId, discardMerged: true);
  if (mergeResult.IsFailure)
      return (Result<Response>)mergeResult.Errors;
  ```

### Test Updates (REQ)

- **REQ-T01**: Update the `RecalculateTotals_ShouldIncludeLineItemAdjustments` test in `Order.Method.Tests.cs` to assert the Result:
  ```csharp
  // Before
  order.RecalculateTotals();

  // After
  var result = order.RecalculateTotals();
  result.IsSuccess.Should().BeTrue();
  ```

### Constraints

- **CON-001**: `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- **CON-002**: No behavioral change. `RecalculateTotals` and `UpdatePaymentState` always succeed — the Result wrapper documents this contract.
- **CON-003**: No new cross-module references.
- **CON-004**: Caller updates outside the Ordering domain (e.g., Shipping, Payment modules) are out of scope. Only Ordering-domain callers are updated per REQ-C02, REQ-C03, REQ-C04.
- **CON-005**: The `Recalculated`, `PaymentStateUpdated`, and `Merged` success messages must follow the existing pattern: `$"Order with ID '{id}' ..."`.

### Guidelines

- **GUD-001**: When propagating errors in feature handlers, cast the Result to the handler's response type: `return (Result<Response>)recalcResult.Errors;` if the handler's return type is `Result<Response>`.
- **GUD-002**: In seeders, the Result can be discarded with a comment explaining why. Seeders are initialization code, not domain operations.
- **GUD-003**: Query/accessor methods (returning `bool`, `string`, `int`) are exempt from the Result requirement. Only mutators require `Result`.

## 4. Interfaces & Data Contracts

### Changed Method Signatures

```
Before:
  OrderMethod.RecalculateTotals(this Order)          → void
  OrderMethod.UpdatePaymentState(this Order)         → void
  OrderMethod.Merge(this Order, Order, Guid?, bool)  → void

After:
  OrderMethod.RecalculateTotals(this Order)          → Result
  OrderMethod.UpdatePaymentState(this Order)         → Result
  OrderMethod.Merge(this Order, Order, Guid?, bool)  → Result
```

### New Success Factories

```csharp
// In OrderResult.Success
public static string Recalculated(Guid id) => $"Order with ID '{id}' totals were recalculated.";
public static string PaymentStateUpdated(Guid id) => $"Order with ID '{id}' payment state was updated.";
```

### Caller Update Pattern

Every bare `cart.RecalculateTotals();` call site becomes:

```csharp
var recalcResult = cart.RecalculateTotals();
if (recalcResult.IsFailure)
    return recalcResult.Errors;  // or cast to handler's Result<T> type
```

## 5. Acceptance Criteria

- **AC-01**: `RecalculateTotals()` returns `Result`. The return statement is `return Result.Ok(OrderResult.Success.Recalculated(order.Id));`.
- **AC-02**: `UpdatePaymentState()` returns `Result`. The return statement is `return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));`.
- **AC-03**: `Merge()` returns `Result`. The return statement is `return Result.Ok(OrderResult.Success.Merged(order.Id));`.
- **AC-04**: `Finalize()` in `StateMachine.cs` captures the `RecalculateTotals()` result and propagates errors.
- **AC-05**: All 12 feature-handler `RecalculateTotals()` call sites follow the `var r = ...; if (r.IsFailure) return r.Errors;` pattern.
- **AC-06**: `Order.Seeder.cs` line 114 uses `order.UpdatePaymentState();` with the Result discarded (comment present).
- **AC-07**: `AssociateCartWithUser.cs` captures the `Merge()` result and propagates errors.
- **AC-08**: The `RecalculateTotals_ShouldIncludeLineItemAdjustments` test asserts `result.IsSuccess.Should().BeTrue()`.
- **AC-09**: `dotnet build` passes with zero warnings.
- **AC-10**: All existing unit tests continue to pass.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for domain logic.
- **Frameworks**: xUnit, FluentAssertions (existing project standards).
- **Test Data Management**: Inline test data via `OrderMethod.Create()` factory.
- **CI/CD Integration**: `dotnet test service/Api/tests/Module.UnitTests --no-restore` must pass.
- **Coverage Requirements**: No behavioral change — coverage is unchanged.
- **Performance Testing**: Not applicable.

### Test File Changes

One test method updated in `Order.Method.Tests.cs`:

```csharp
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
    var result = order.RecalculateTotals();
    result.IsSuccess.Should().BeTrue();
    order.AdjustmentTotal.Should().Be(7m);
    order.ItemTotal.Should().Be(10m);
    order.Total.Should().Be(17m);
    order.OutstandingBalance.Should().Be(17m);
}
```

## 7. Rationale & Context

### Why change void → Result when these methods never fail?

The non-negotiable rule states "all domain operations return `Result<T>` or `Result`". `RecalculateTotals`, `UpdatePaymentState`, and `Merge` are domain operations — they mutate entity state. The fact that they never fail today is an implementation detail, not a contract. Returning `Result`:

1. **Documents the domain contract**: The caller sees a `Result` return and knows to check for errors. Today's implementation never returns an error, but tomorrow's might add validation (e.g., overflow detection, negative quantity checks).
2. **Enables future validation**: If a constraint is added (e.g., `RecalculateTotals` detecting an overflow in Total), callers already handle the `Result` — no signature change needed.
3. **Consistency**: All other state-machine methods (`Finalize`, `Cancel`, `Resume`, `Approve`, `Empty`, `Delete`) return `Result`. The 3 void methods are outliers that break the mental model.

### Why not change query/accessor methods?

`CheckoutAllowed()`, `DeliveryRequired()`, `CanShip()`, `RequireEmail()`, etc. return `bool` because they answer a yes/no question about current state. They do not mutate the entity. The Result-only rule applies to mutators. This distinction is consistent with the codebase's use of `IsDeleted` (property) vs `Delete()` (Result-returning mutator).

### Why discard the Result in the seeder?

The seeder directly writes entities to the database via `Context.Set<Order>().Add(order)`. It is initialization code, not feature-logic. If `UpdatePaymentState` ever gained a failure path, the seeder would still be correct because it writes the entity regardless of the derived payment state. A comment explains this design choice.

## 8. Dependencies & External Integrations

### Internal Dependencies

- **EXT-001**: `OrderResult.Success.Recalculated(Guid)` and `OrderResult.Success.PaymentStateUpdated(Guid)` must be added to `Order.Result.cs` before the method signatures change.
- **EXT-002**: `OrderResult.Success.Merged(Guid)` already exists in `Order.Result.cs` (added in dead-code removal spec). No changes needed.
- **EXT-003**: `Finalize()` in `StateMachine.cs` internally calls `order.RecalculateTotals()` — this is the only domain-internal caller.

### Infrastructure Dependencies

- **INF-001**: No new infrastructure dependencies.

### Technology Platform Dependencies

- **PLT-001**: .NET 10, C# preview. `Result` implicit conversion from `Error[]` to `Result` must work for error propagation in callers.

## 9. Examples & Edge Cases

### Returning Result from a never-failing computation

```csharp
// In Order.Method.Computation.cs
public static Result RecalculateTotals(this Order order)
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
    return Result.Ok(OrderResult.Success.Recalculated(order.Id));
}
```

### Casting errors in feature handlers with typed Result<T>

```csharp
// In feature handler returning Result<Response>
var recalcResult = cart.RecalculateTotals();
if (recalcResult.IsFailure)
    return (Result<Response>)recalcResult.Errors;
```

### Discarding Result in seeder (intentional)

```csharp
// In Order.Seeder.cs
order.UpdatePaymentState(); // Result unused — seeder writes domain state directly
Context.Set<Order>().Add(order);
```

## 10. Validation Criteria

- **VAL-01**: `dotnet build` exits with code 0 and zero warnings.
- **VAL-02**: `dotnet test service/Api/tests/Module.UnitTests --no-restore` passes all tests.
- **VAL-03**: `rg "public static void" service/Api/src/Module/Ordering/Domain/Orders/` returns zero results (no void mutators remain).
- **VAL-04**: `RecalculateTotals` returns `Result` — verified via `rg "public static Result RecalculateTotals" service/Api/src/Module/Ordering/Domain/Orders/`.
- **VAL-05**: `UpdatePaymentState` returns `Result` — verified via grep.
- **VAL-06**: `Merge` returns `Result` — verified via grep.
- **VAL-07**: No bare `RecalculateTotals();` calls remain in feature handlers — verified via `rg "RecalculateTotals\(\);" service/Api/src/Module/Ordering/Features/` returning zero results.
- **VAL-08**: `OrderResult.Success` contains `Recalculated(Guid)` and `PaymentStateUpdated(Guid)`.
- **VAL-09**: `OrderResult.Success` already contains `Merged(Guid)` (pre-existing).
- **VAL-10**: The `RecalculateTotals_ShouldIncludeLineItemAdjustments` test asserts `result.IsSuccess.Should().BeTrue()`.

## 11. Related Specifications / Further Reading

- [spec-design-order-domain-concern-consolidation.md](./spec-design-order-domain-concern-consolidation.md) — Dead-code removal and concern consolidation (v2.0).
- [spec-design-order-domain-convention-fixes.md](./spec-design-order-domain-convention-fixes.md) — Initial convention migration to `Order.Method.{Concern}.cs`.
- [.harness/principles.yml](../.harness/principles.yml) — Result objects rule (section 1).
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md) — Domain file conventions.
