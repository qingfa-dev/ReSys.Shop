---
title: Ordering Domain — Feature Handler Domain Logic Extraction
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
tags: design, ordering, domain, feature-bypass, service/Api
---

# Introduction

A comprehensive audit of all 27 Ordering feature handlers revealed that 14 handlers directly mutate `Order` / `LineItem` properties instead of calling domain methods, 4 handlers completely bypass existing domain methods (`Delete`, `Cancel`, `Finalize`), multiple handlers duplicate identical business logic blocks, and 2 existing domain methods are incomplete (missing timestamps that handlers patch externally). Additionally, 19 domain methods need to be created to encapsulate inline business logic.

This specification defines: (1) wiring existing domain methods into handlers that currently bypass them, (2) enhancing incomplete domain methods, (3) creating 19 new domain methods, and (4) updating all 22 affected handlers to use them.

## 1. Purpose & Scope

**Purpose**: Extract all domain logic from Ordering feature handlers into proper domain methods, eliminate handler-side property mutation, wire existing but unused domain methods, enhance incomplete domain methods, and create missing domain methods.

**Scope**: All 27 handler files under `service/Api/src/Module/Ordering/Features/` that mutate `Order` or `LineItem` state, plus 3 domain files (`Order.Method.StateMachine.cs`, `Order.Method.Checkout.cs`, `Order.Method.Contents.cs` — to be renamed/repurposed). No changes to `Adjustments/`, non-Ordering modules, or Infrastructure.

**Audience**: Backend developers and coding agents implementing the extraction.

**Assumptions**:
- All prior specs (convention migration, dead-code removal, Result pattern compliance) are complete.
- `OrderMethod` is a `static partial class` — new methods can be added to any concern file.
- The `Order.Method.Contents.cs` file no longer exists (deleted in dead-code removal). A new `Order.Method.Operations.cs` file will be created for the operation methods.
- Persistence operations (`dbContext.Set.Add/Remove`, `SaveChangesAsync`) remain handler responsibilities.

## 2. Definitions

| Term | Definition |
|------|-----------|
| **Feature bypass** | A handler that directly mutates entity properties instead of calling an existing domain method that performs the same mutation. |
| **Property leakage** | A handler that directly sets an entity property without any domain method existing for that operation. |
| **Duplicated logic** | Identical business logic appearing in two or more handlers without being extracted into a shared domain method. |
| **Incomplete method** | A domain method that performs part of an operation, requiring the caller to manually set additional properties after invocation. |
| **Query method** | A `bool`-returning method on the domain entity that answers a yes/no question about entity state without mutation. |

## 3. Requirements, Constraints & Guidelines

---

### Part A: Wire Existing Domain Methods Into Bypassing Handlers (REQ-W)

These 4 handlers bypass existing domain methods — the methods exist, the handlers just don't call them.

- **REQ-W01**: `DeleteCart.cs` (lines 39-40) sets `cart.IsDeleted = true; cart.DeletedAtUtc = DateTimeOffset.UtcNow` — replace with `cart.Delete(currentUser.UserName ?? "System")`. The domain method `Delete()` at `StateMachine.cs:106` already enforces the Draft/Expired guard and handles `DeletedBy`.

- **REQ-W02**: `DeleteOrder.cs` (lines 29-30) sets `order.IsDeleted = true; order.DeletedAtUtc = DateTimeOffset.UtcNow` — replace with `order.Delete(deletedBy)`. Remove redundant status check at line 25-26 (already inside `Delete()`).

- **REQ-W03**: `CancelOrder.cs` (lines 51-55) manually sets `Status = Canceled`, `CanceledAtUtc`, `CanceledById` — replace with `entity.Cancel(canceledById)`. The domain method `Cancel()` at `StateMachine.cs:35` already performs all three mutations. Remove redundant `AlreadyCanceled` check at lines 46-47.

- **REQ-W04**: `UpdateOrderStatus.cs` (lines 40-49) has a status switchboard: the `Placed` case manually sets `Status = Placed` instead of calling `entity.Finalize()`, and the `Canceled` case manually sets `Status = Canceled` + timestamps instead of calling `entity.Cancel(userId)`. Replace both branches with the domain method calls.

- **REQ-W05**: `ValidateCheckout.cs` (line 36-37) checks `cart.LineItems.Count == 0` inline instead of using the existing `cart.CheckoutAllowed()` at `Checkout.cs:66`. Replace with `cart.CheckoutAllowed()`.

- **REQ-W06**: `AddToCart.cs` (line 114-115) manually increments `existingLine.Quantity` and computes `existingLine.Total = Price * Quantity` instead of calling `existingLine.RecalculateTotal()`. Use `existingLine.UpdateQuantity(existingLine.Quantity + request.Quantity)` which handles both assignment + recalculate + range validation.

- **REQ-W07**: `UpdateCartItemQuantity.cs` (lines 54-55) duplicates `lineItem.UpdateQuantity()` — manually sets `Quantity` and computes `Total = Price * Quantity`. Replace with `lineItem.UpdateQuantity(request.Quantity)`.

---

### Part B: Enhance Incomplete Domain Methods (REQ-E)

These 2 domain methods perform part of an operation. Handlers patch the remaining state externally.

- **REQ-E01**: `Finalize()` at `StateMachine.cs:12` sets `Status = Placed` and `CompletedAtUtc` but does NOT set `CheckoutState = Complete`. The checkout handler and `UpdateOrderStatus` handler both set `CheckoutState` externally. Add `order.CheckoutState = CheckoutState.Complete;` inside `Finalize()` after `order.RecalculateTotals();`.

- **REQ-E02**: `Approve()` at `StateMachine.cs:69` sets `ApprovedById` but does NOT set `ApprovedAtUtc` or `ModifiedAtUtc`. The `ApproveOrder.cs` handler sets both externally. Add inside `Approve()`:
  ```csharp
  order.ApprovedAtUtc = DateTimeOffset.UtcNow;
  order.ModifiedAtUtc = DateTimeOffset.UtcNow;
  ```

---

### Part C: Create New Domain Methods (REQ-N)

19 new domain methods organized by concern file placement.

#### Order.Method.StateMachine.cs additions:

- **REQ-N01**: `Place(string orderNumber)` — full order placement ceremony. Validates checkout prerequisites (state >= Confirm, addresses set, shipping method set, email set, line items present, no discontinued variants), sets `Status = Placed`, `CheckoutState = Complete`, `CompletedAtUtc`, `Number = orderNumber`. Replaces `CreateOrderFromCart.cs:55-115`.

- **REQ-N02**: `Complete(string modifiedBy)` — transitions from Placed to final complete state. Guard: only Placed orders. Sets `CheckoutState = Complete`, `CompletedAtUtc`, `ModifiedBy`. Replaces `CompleteOrder.cs:26-33`.

#### Order.Method.Checkout.cs additions:

- **REQ-N03**: `ValidateCheckoutPrerequisites()` — returns `Result`. Checks: CheckoutState >= Confirm, both addresses present, shipping method present, email present, line items present, no discontinued variants. Replaces `CreateOrderFromCart.cs:55-68, 95-106`.

- **REQ-N04**: `MarkPaymentAsPaid()` — returns `Result`. Sets `PaymentState = OrderConstant.PaymentState.Paid`. Replaces `CreateOrderFromCart.cs:91`.

- **REQ-N05**: `UpdateDetails(string? email, string? instructions, Guid? billAddressId, Guid? shipAddressId, Guid? shippingMethodId)` — returns `Result`. Guard: only Draft orders. Applies all non-null values, sets `ModifiedAtUtc`. Replaces `UpdateOrderAdmin.cs:26-37` and `UpdateCheckout.cs:42-46`.

- **REQ-N06**: `SetBillAddress(Guid addressId)` — returns `Result`. Guard: only Draft orders. Sets `BillAddressId`, `ModifiedAtUtc`. Replaces `UpdateOrderBillAddress.cs:26-31`.

- **REQ-N07**: `SetShipAddress(Guid addressId)` — returns `Result`. Guard: only Draft orders. Sets `ShipAddressId`, `ModifiedAtUtc`. Replaces `UpdateOrderShipAddress.cs:26-31`.

- **REQ-N08**: `SetShippingMethod(Guid methodId)` — returns `Result`. Sets `ShippingMethodId`, resets `ShipmentTotal = 0`, sets `ModifiedAtUtc`, calls `RecalculateTotals()`. Replaces `UpdateOrderShippingMethod.cs:26-31`.

- **REQ-N09**: `HasAddresses()` — returns `bool`. `BillAddressId != null && ShipAddressId != null`. Replaces `ValidateCheckout.cs:40-41`.

- **REQ-N10**: `HasShippingMethod()` — returns `bool`. `ShippingMethodId != null`. Replaces `ValidateCheckout.cs:44-45`.

- **REQ-N11**: `HasEmail()` — returns `bool`. `!string.IsNullOrWhiteSpace(Email)`. Replaces `ValidateCheckout.cs:48-49`.

- **REQ-N12**: `CanModifyLineItems()` — returns `bool`. `Status == OrderStatus.Draft`. Replaces `UpdateOrderLineItem.cs:25-26`.

#### Order.Method.Operations.cs (new file) additions:

This file replaces the deleted `Order.Method.Contents.cs` with methods for cart-contents operations that go beyond simple add/remove (which were dead and removed).

- **REQ-N13**: `ValidatePayment(decimal amount)` — returns `Result`. Returns `PaymentRequired` if `Total > 0` and `amount` is 0. Returns `PaymentAmountMismatch` if `amount != Total`. Returns `PaymentFailed` if amount matches but payment source isn't confirmed (accepts a `bool isPaymentConfirmed` parameter). Replaces `CreateOrderFromCart.cs:71-88`.

- **REQ-N14**: `AddLineItem(LineItem lineItem)` — returns `Result<LineItem>`. Guard: only Draft orders. Validates `MaxLineItems` constant. Adds to `LineItems` collection, calls `RecalculateTotals()`. Replaces `AddOrderLineItem.cs:26,33`.

- **REQ-N15**: `RemoveLineItem(Guid lineItemId)` — returns `Result<LineItem>`. Guard: only Draft orders. Finds by ID, removes from collection, calls `RecalculateTotals()`, returns the removed item for EF disposal. Replaces `RemoveCartItem.cs:42-43` and `RemoveOrderLineItem.cs:27-29`.

- **REQ-N16**: `ReplaceShippingAdjustment(decimal cost, Guid shippingMethodId, Guid orderId)` — returns `Result`. Atomic: removes all existing shipping-typed adjustments from collection, creates a new `Adjustment` for the given cost, adds to collection, calls `RecalculateTotals()`. Replaces the duplicated ~30-line block in `SelectShippingRate.cs:66-93` and `UpdateCheckout.cs:49-93`.

- **REQ-N17**: `CalculateTotalWeight(Dictionary<Guid, decimal> variantWeights)` — returns `decimal`. Sums `li.Quantity * variantWeights[li.VariantId]` for all line items. Returns 0 if a variant is missing from the dictionary. Replaces `SelectShippingRate.cs:43-51`.

- **REQ-N18**: `TransferOwnership(Guid userId)` — returns `Result`. Sets `UserId = userId`, clears `SessionId = null`, sets `ModifiedAtUtc`. Guard: order must be in Draft status. Replaces `AssociateCartWithUser.cs:53-54`.

- **REQ-N19**: `RemoveShippingAdjustments()` — helper used by `ReplaceShippingAdjustment`. Clears all adjustments where `SourceType == "Shipping"` from the collection. Could be `internal` if only called by `ReplaceShippingAdjustment`.

---

### Part D: Update Callers (REQ-C)

Each new or enhanced domain method requires updating its caller handlers. The mapping is:

| Method | Handlers Updated |
|--------|-----------------|
| `Place(orderNumber)` | `CreateOrderFromCart.cs` — replaces lines 55-115 |
| `Complete(modifiedBy)` | `CompleteOrder.cs` — replaces lines 26-33 |
| `ValidateCheckoutPrerequisites()` | `CreateOrderFromCart.cs` — replaces lines 55-68, 95-106 |
| `MarkPaymentAsPaid()` | `CreateOrderFromCart.cs` — replaces line 91 |
| `ValidatePayment(amount, isConfirmed)` | `CreateOrderFromCart.cs` — replaces lines 71-88 |
| `UpdateDetails(...)` | `UpdateOrderAdmin.cs` — replaces lines 26-37; `UpdateCheckout.cs` — replaces lines 42-46 |
| `SetBillAddress(...)` | `UpdateOrderBillAddress.cs` — replaces lines 26-31 |
| `SetShipAddress(...)` | `UpdateOrderShipAddress.cs` — replaces lines 26-31 |
| `SetShippingMethod(...)` | `UpdateOrderShippingMethod.cs` — replaces lines 26-31 |
| `AddLineItem(...)` | `AddOrderLineItem.cs` — replaces line 26 |
| `RemoveLineItem(...)` | `RemoveCartItem.cs`, `RemoveOrderLineItem.cs` |
| `ReplaceShippingAdjustment(...)` | `SelectShippingRate.cs` — replaces lines 66-93; `UpdateCheckout.cs` — replaces lines 49-93 |
| `CalculateTotalWeight(...)` | `SelectShippingRate.cs` — replaces lines 43-51; `UpdateCheckout.cs` |
| `TransferOwnership(...)` | `AssociateCartWithUser.cs` — replaces lines 53-54 |
| `HasAddresses()` / `HasShippingMethod()` / `HasEmail()` | `ValidateCheckout.cs` — replaces lines 40-49 |
| `CanModifyLineItems()` | `UpdateOrderLineItem.cs` — replaces lines 25-26 |

### Constraints

- **CON-001**: `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- **CON-002**: No new cross-module references. New domain methods stay in `Module.Ordering.Domain.Orders` namespace.
- **CON-003**: All new mutator methods must return `Result` or `Result<T>`. Query methods return `bool` or `decimal`.
- **CON-004**: Persistence operations (`dbContext.Set<>.Add/Remove`, `SaveChangesAsync`) remain in handlers.
- **CON-005**: New methods follow existing naming: `{Verb}{Noun}` for mutators (`SetBillAddress`, `RemoveLineItem`), `Can{Action}` / `Has{Property}` for queries.
- **CON-006**: Guard clauses use `OrderResult.Errors.{Name}` — add new error factories if needed.

### Guidelines

- **GUD-001**: New error codes follow `Order.{Aggregate}.{Problem}` pattern.
- **GUD-002**: `ValidatePayment` may reference `PaymentCapture` types — this is acceptable because the method accepts a simple `decimal amount + bool isConfirmed` signature, keeping the dependency one-way.
- **GUD-003**: `CalculateTotalWeight` accepts a `Dictionary<Guid, decimal>` to avoid depending on `Variant` type — the handler builds the lookup.

## 4. Interfaces & Data Contracts

### New Method Signatures

```
// StateMachine additions
OrderMethod.Place(this Order, string orderNumber)                         → Result
OrderMethod.Complete(this Order, string modifiedBy)                       → Result

// Checkout additions
OrderMethod.ValidateCheckoutPrerequisites(this Order)                     → Result
OrderMethod.MarkPaymentAsPaid(this Order)                                 → Result
OrderMethod.UpdateDetails(this Order, string? email, string? instructions,
    Guid? billAddressId, Guid? shipAddressId, Guid? shippingMethodId)     → Result
OrderMethod.SetBillAddress(this Order, Guid addressId)                    → Result
OrderMethod.SetShipAddress(this Order, Guid addressId)                    → Result
OrderMethod.SetShippingMethod(this Order, Guid methodId)                  → Result
OrderMethod.HasAddresses(this Order)                                      → bool
OrderMethod.HasShippingMethod(this Order)                                 → bool
OrderMethod.HasEmail(this Order)                                          → bool
OrderMethod.CanModifyLineItems(this Order)                                → bool

// Operations additions (new file)
OrderMethod.ValidatePayment(this Order, decimal amount, bool isConfirmed) → Result
OrderMethod.AddLineItem(this Order, LineItem lineItem)                    → Result<LineItem>
OrderMethod.RemoveLineItem(this Order, Guid lineItemId)                   → Result<LineItem>
OrderMethod.ReplaceShippingAdjustment(this Order, decimal cost,
    Guid shippingMethodId, Guid orderId)                                  → Result
OrderMethod.CalculateTotalWeight(this Order,
    Dictionary<Guid, decimal> variantWeights)                             → decimal
OrderMethod.TransferOwnership(this Order, Guid userId)                    → Result
```

### New Error Factories

Add to `OrderResult.Errors`:

```csharp
/// <summary>Line item with the specified ID was not found.</summary>
public static Error LineItemNotFound(Guid id) => Error.NotFound(
    code: "Order.LineItem.NotFound",
    message: $"Line item with ID '{id}' was not found.");

/// <summary>Payment has not been confirmed by the gateway.</summary>
public static Error PaymentNotConfirmed => Error.Validation(
    code: "Order.Payment.NotConfirmed",
    message: "Payment has not been confirmed by the gateway.");

/// <summary>Shipping adjustment was not found.</summary>
public static Error ShippingAdjustmentNotFound => Error.NotFound(
    code: "Order.ShippingAdjustment.NotFound",
    message: "Shipping adjustment was not found.");

/// <summary>Order is not in Draft status and cannot be modified.</summary>
public static Error NotDraft => Error.Validation(
    code: "Order.Update.NotDraft",
    message: "Only draft orders can be modified.");
```

Note: `NotDraft` already exists at `OrderResult.Errors.NotDraft` (code `Order.Update.NotDraft`). Verify and reuse.

### File Map

| File | New Methods |
|------|------------|
| `Order.Method.StateMachine.cs` | `Place`, `Complete` |
| `Order.Method.Checkout.cs` | `ValidateCheckoutPrerequisites`, `MarkPaymentAsPaid`, `UpdateDetails`, `SetBillAddress`, `SetShipAddress`, `SetShippingMethod`, `HasAddresses`, `HasShippingMethod`, `HasEmail`, `CanModifyLineItems` |
| `Order.Method.Operations.cs` (NEW) | `ValidatePayment`, `AddLineItem`, `RemoveLineItem`, `ReplaceShippingAdjustment`, `CalculateTotalWeight`, `TransferOwnership` |

## 5. Acceptance Criteria

### Wiring Existing Methods

- **AC-01**: `DeleteCart.cs` calls `cart.Delete(deletedBy)` instead of directly setting `IsDeleted`/`DeletedAtUtc`.
- **AC-02**: `DeleteOrder.cs` calls `order.Delete(deletedBy)` instead of directly setting `IsDeleted`/`DeletedAtUtc`. Redundant status check removed.
- **AC-03**: `CancelOrder.cs` calls `entity.Cancel(canceledById)` instead of manually setting `Status`/`CanceledAtUtc`/`CanceledById`. Redundant guard removed.
- **AC-04**: `UpdateOrderStatus.cs` calls `entity.Finalize()` and `entity.Cancel(userId)` instead of directly setting status/timestamps.
- **AC-05**: `ValidateCheckout.cs` uses `cart.CheckoutAllowed()` instead of inline `LineItems.Count == 0`.
- **AC-06**: `AddToCart.cs` uses `existingLine.UpdateQuantity(...)` instead of manual `Quantity +=` and `Total = Price * Quantity`.
- **AC-07**: `UpdateCartItemQuantity.cs` uses `lineItem.UpdateQuantity(request.Quantity)` instead of manual `Quantity = `/`Total = `.

### Enhanced Methods

- **AC-08**: `Finalize()` sets `CheckoutState = CheckoutState.Complete` internally.
- **AC-09**: `Approve()` sets `ApprovedAtUtc` and `ModifiedAtUtc` internally.

### New Methods

- **AC-10**: `Place(orderNumber)` performs all validations and sets `Status`, `CheckoutState`, `CompletedAtUtc`, `Number`.
- **AC-11**: `Complete(modifiedBy)` guards for Placed-only and sets `CheckoutState`, `CompletedAtUtc`, `ModifiedBy`.
- **AC-12**: `ValidateCheckoutPrerequisites()` returns `Result` with appropriate error for each missing prerequisite.
- **AC-13**: `UpdateDetails(...)` applies partial updates with Draft-only guard.
- **AC-14**: `SetBillAddress()` / `SetShipAddress()` / `SetShippingMethod()` each have Draft-only guards.
- **AC-15**: `HasAddresses()` / `HasShippingMethod()` / `HasEmail()` / `CanModifyLineItems()` return correct `bool`.
- **AC-16**: `ValidatePayment(amount, isConfirmed)` checks amount match and confirmation status.
- **AC-17**: `AddLineItem(lineItem)` adds to collection, enforces `MaxLineItems`, calls `RecalculateTotals()`.
- **AC-18**: `RemoveLineItem(lineItemId)` removes from collection, calls `RecalculateTotals()`, returns removed item.
- **AC-19**: `ReplaceShippingAdjustment(cost, shippingMethodId, orderId)` atomically removes old + adds new adjustment.
- **AC-20**: `CalculateTotalWeight(variantWeights)` sums `quantity * weight` for all line items.
- **AC-21**: `TransferOwnership(userId)` sets `UserId`, clears `SessionId`.

### Build & Test

- **AC-22**: `dotnet build` passes with zero warnings.
- **AC-23**: All 2388+ existing tests pass.
- **AC-24**: New domain methods have unit tests covering success + each guard clause.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for new domain methods; integration tests for handler wiring.
- **Frameworks**: xUnit, FluentAssertions, Moq.
- **Test Data Management**: `OrderMethod.Create("USD", userId, storeId)` factory for all tests.
- **CI/CD Integration**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Order"` must pass.
- **Coverage Requirements**: Each new method must have at least: 1 success test + 1 test per guard clause.
- **Performance Testing**: Not applicable.

### Test per New Method

| Method | Tests |
|--------|-------|
| `Place(orderNumber)` | Success (valid order + payment), fails when missing addresses, fails when missing shipping, fails when empty, fails when email missing |
| `Complete(modifiedBy)` | Success (Placed order), fails when not Placed |
| `ValidateCheckoutPrerequisites()` | Success, fails per prerequisite |
| `MarkPaymentAsPaid()` | Success (sets state) |
| `ValidatePayment(amount, confirmed)` | Success, fails when amount mismatch, fails when not confirmed |
| `UpdateDetails(...)` | Success (Draft), fails when not Draft |
| `SetBillAddress(addressId)` | Success (Draft), fails when not Draft |
| `SetShipAddress(addressId)` | Success (Draft), fails when not Draft |
| `SetShippingMethod(methodId)` | Success (resets ShipmentTotal + recalculates) |
| `AddLineItem(lineItem)` | Success, fails when not Draft, fails when MaxLineItems exceeded |
| `RemoveLineItem(lineItemId)` | Success, fails when not found, fails when not Draft |
| `ReplaceShippingAdjustment(...)` | Success, creates new adjustment |
| `CalculateTotalWeight(dict)` | Correct sum, returns 0 for missing variant |
| `TransferOwnership(userId)` | Success (sets UserId + clears SessionId) |

## 7. Rationale & Context

### Why 19 new methods?

The previous convention migration and dead-code removal reduced the domain surface to 10 methods — the minimum viable set of state-machine methods. The feature handlers were never updated to use these methods; they continued to directly mutate entity properties via arbitrary `set` accessors. This creates three problems:

1. **Domain invariants are unenforceable**: `Delete()` guards against non-Draft/Expired status, but `DeleteCart.cs` directly sets `IsDeleted = true` without any status check. The guard exists in code but is never executed in production.
2. **Duplicate logic drifts**: `SelectShippingRate.cs` and `UpdateCheckout.cs` contain identical 30-line shipping adjustment blocks. If one is fixed, the other silently diverges.
3. **Handler bloat**: `CreateOrderFromCart.cs` is 219 lines, of which ~70 lines are domain logic (validation, state transitions, payment checks) that should be 3 domain method calls.

### Why `Place()` instead of expanding `Finalize()`?

`Finalize()` is a low-level state transition (Draft → Placed). `Place()` is a high-level checkout ceremony that validates prerequisites, confirms payment, transitions state, and assigns the order number. These are different abstraction levels. `Place()` calls `ValidateCheckoutPrerequisites()` then `Finalize()` internally, keeping the ceremony explicit and testable.

### Why `Operations.cs` instead of resurrecting `Contents.cs`?

The deleted `Contents.cs` contained `AddItem`/`RemoveItem`/`RemoveLineItem` — dead methods with zero callers. The new `Operations.cs` contains methods that feature handlers actually need (`AddLineItem`, `RemoveLineItem`, `ReplaceShippingAdjustment`, `CalculateTotalWeight`, `ValidatePayment`, `TransferOwnership`). The name "Operations" reflects that these are multi-step domain operations spanning multiple aggregates (LineItem, Adjustment), not just cart-contents manipulation.

### Why `ValidatePayment` accepts `decimal + bool` instead of a `PaymentCapture` object?

The `PaymentCapture` type lives in `Module.Payment.Domain` — cross-module references are forbidden. The handler queries the payment and passes primitive values to the domain method, keeping the domain free of Payment module dependencies.

## 8. Dependencies & External Integrations

### Internal Dependencies

- **EXT-001**: `ReplaceShippingAdjustment()` creates `Adjustment` entities — it must import `Module.Ordering.Domain.Adjustments` namespace.
- **EXT-002**: `AddLineItem()` and `RemoveLineItem()` reference `LineItem` — already accessible since `LineItems` live in the same `Domain` parent directory.
- **EXT-003**: `CalculateTotalWeight()` accepts a `Dictionary<Guid, decimal>` built by the handler from `Variant` queries — no Catalog module dependency.
- **EXT-004**: `ValidatePayment()` accepts a `decimal amount + bool isConfirmed` — no Payment module dependency.

### Infrastructure Dependencies

- **INF-001**: No new infrastructure dependencies.

### Technology Platform Dependencies

- **PLT-001**: .NET 10, C# preview. All methods are extension methods on `Order` in `static partial class OrderMethod`.

## 9. Examples & Edge Cases

### `Place()` — the full checkout ceremony

```csharp
public static Result Place(this Order order, string orderNumber)
{
    var prerequisites = order.ValidateCheckoutPrerequisites();
    if (prerequisites.IsFailure)
        return prerequisites.Errors;

    order.Status = OrderStatus.Placed;
    order.CheckoutState = CheckoutState.Complete;
    order.CompletedAtUtc = DateTimeOffset.UtcNow;
    order.Number = orderNumber;
    order.RecalculateTotals();

    return Result.Ok(OrderResult.Success.Placed(order.Id));
}
```

### `ReplaceShippingAdjustment()` — atomic remove + add

```csharp
public static Result ReplaceShippingAdjustment(this Order order, decimal cost, Guid shippingMethodId, Guid orderId)
{
    var toRemove = order.Adjustments
        .Where(a => a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
        .ToList();
    foreach (var adj in toRemove)
        order.Adjustments.Remove(adj);

    var newAdj = AdjustmentMethod.Create(
        label: $"Shipping — {cost:F2}",
        amount: cost,
        adjustableId: order.Id,
        adjustableType: AdjustmentConstant.AdjustableTypes.Order,
        sourceId: shippingMethodId,
        sourceType: AdjustmentConstant.SourceTypes.Shipping,
        orderId: orderId);
    if (newAdj.IsFailure)
        return newAdj.Errors;

    order.Adjustments.Add(newAdj.Value);
    order.RecalculateTotals();

    return Result.Ok(OrderResult.Success.Updated(order.Id));
}
```

### `TransferOwnership()` — cart reassignment

```csharp
public static Result TransferOwnership(this Order order, Guid userId)
{
    if (order.Status != OrderStatus.Draft)
        return OrderResult.Errors.NotDraft;

    order.UserId = userId;
    order.SessionId = null;
    order.ModifiedAtUtc = DateTimeOffset.UtcNow;

    return Result.Ok(OrderResult.Success.Updated(order.Id));
}
```

### Handler calls domain method — before vs after

Before (DeleteCart.cs, bypass):
```csharp
cart.IsDeleted = true;
cart.DeletedAtUtc = DateTimeOffset.UtcNow;
```

After:
```csharp
var deleteResult = cart.Delete(currentUser.UserName ?? "System");
if (deleteResult.IsFailure)
    return deleteResult.Errors;
```

## 10. Validation Criteria

- **VAL-01**: `dotnet build` exits with code 0 and zero warnings.
- **VAL-02**: `dotnet test service/Api/tests/Module.UnitTests --no-restore` passes all tests.
- **VAL-03**: `Order.Method.Operations.cs` exists with all 6 methods.
- **VAL-04**: `Order.Method.StateMachine.cs` contains `Place` and `Complete`.
- **VAL-05**: `Order.Method.Checkout.cs` contains all 10 new methods (6 mutators + 4 queries).
- **VAL-06**: No handler directly sets `Order.IsDeleted`, `Order.DeletedAtUtc`, `Order.Status` (outside domain method calls) — verified via rg.
- **VAL-07**: No handler directly sets `LineItem.Quantity`, `LineItem.Total` (outside `UpdateQuantity` / `RecalculateTotal`) — verified via rg.
- **VAL-08**: `Finalize()` contains `CheckoutState = CheckoutState.Complete` — grep check.
- **VAL-09**: `Approve()` contains `ApprovedAtUtc` and `ModifiedAtUtc` — grep check.
- **VAL-10**: The duplicated shipping adjustment block exists only in `Order.Method.Operations.cs` — zero occurrences in feature handlers.
- **VAL-11**: `CancelOrder.cs` does not contain `entity.Status = OrderStatus.Canceled` — it uses `entity.Cancel()`.
- **VAL-12**: New domain methods all return `Result` or `Result<T>` — no `void` mutators.

## 11. Related Specifications / Further Reading

- [spec-design-order-domain-concern-consolidation.md](./spec-design-order-domain-concern-consolidation.md) — Dead-code removal and concern consolidation (v2.0).
- [spec-design-order-result-pattern-compliance.md](./spec-design-order-result-pattern-compliance.md) — Result pattern compliance for void methods.
- [spec-design-order-domain-convention-fixes.md](./spec-design-order-domain-convention-fixes.md) — Initial convention migration.
- [.harness/principles.yml](../.harness/principles.yml) — Result objects rule, vertical slice isolation.
