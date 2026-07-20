---
title: Ordering System Defect Remediation — Domain Bugs, Boundary Violations, and Code Quality Fixes
version: 1.0
date_created: 2026-07-20
owner: Platform Team
tags: design, ordering, bugfix, remediation
---

# Introduction

Fix 15 defects discovered in the Ordering module during code review: 2 bugs that cause incorrect financial totals or invalid state transitions, 8 risks that threaten boundary hygiene, inventory accuracy, or future maintainability, and 5 nits that degrade code quality. All fixes comply with the codebase rules: Result objects not exceptions; forward-only dependency between `Shared` and `Module`; no cross-module references in domain layer.

## 1. Purpose & Scope

**Purpose**: Define exact, verifiable changes to eliminate the identified defects in the Ordering module (`service/Api/src/Module/Ordering/`) and adjacent Inventory concerns.

**Scope**: Ordering domain models (`Order`, `LineItem`, `Adjustment`), feature handlers (`CreateOrderFromCart`, `UpdateCartItemQuantity`, `AddToCart`, `ApproveOrder`, `CancelOrder`), background job (`CartExpiryJob`), and Inventory feature (`CheckStockAvailability.Query`). Also affects tests in `Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateItemQuantity/`.

**Out of scope**: Checkout concurrency redesign, payment gateway integration, notification template changes, migration generation (follow-up).

**Audience**: Agents and developers implementing these fixes.

**Assumptions**: `OrderStatus` enum includes `Draft`, `Placed`, `Canceled`, `Expired`, `Completed`. `CheckoutState` enum includes `Address`, `Deliver`, `Payment`, `Confirm`, `Complete`. `AdjustmentConstant.SourceTypes.Shipping` is the string constant `"Shipping"`.

## 2. Definitions

| Term | Definition |
|---|---|
| `RecalculateTotals()` | Domain extension method on `Order` that recomputes `ItemTotal`, `AdjustmentTotal`, `ShipmentTotal`, `Total`, and `OutstandingBalance` from current `LineItems` and `Adjustments` collections. |
| `AdjustmentTotal` | Property on `Order` representing the sum of all non-shipping eligible adjustments plus per-line-item adjustment totals. |
| `ShipmentTotal` | Property on `Order` representing the sum of all eligible `SourceType == "Shipping"` adjustments. |
| Boundary violation | Any `using` directive in the Ordering module that references a type from another module's `Domain` namespace — violates forward-only dependency rule. |
| Dead code | Code paths unreachable at runtime (always-true guards, always-`Ok` Results) or fields always populated with a constant value. |
| Stock reservation | An inventory hold placed via `ReserveCartStock.Command` sent through MediatR to the Inventory module; expires after TTL minutes. |
| wasPlaced | Local variable in `CancelOrder.Handler` set to `entity.Status == OrderStatus.Placed && entity.CompletedAtUtc.HasValue`. |

## 3. Requirements, Constraints & Guidelines

### RM-001: RecalculateTotals must not double-count shipping adjustments

`AdjustmentTotal` sums ALL eligible adjustments (line 22-23 of `Order.Method.Computation.cs`), including those with `SourceType == "Shipping"`. `ShipmentTotal` independently sums shipping adjustments (line 26-28). `Total = ItemTotal + ShipmentTotal + AdjustmentTotal` (line 31) therefore includes shipping costs twice.

**Fix**: Exclude shipping-source adjustments from `AdjustmentTotal` by filtering `SourceType != AdjustmentConstant.SourceTypes.Shipping`:

```csharp
order.AdjustmentTotal =
    order.LineItems.Sum(li => li.AdjustmentTotal) +
    order.Adjustments.Where(a => a.Eligible && a.SourceType != AdjustmentConstant.SourceTypes.Shipping).Sum(a => a.Amount);
```

**Constraint**: This change alters the computed `Total` for any order with shipping adjustments. Existing orders will have inflated totals. A data migration may be needed to recalculate historical order totals.

### RM-002: Approve() must reject non-Placed orders

`Order.Approve(Guid approvedById)` in `Order.Method.StateMachine.cs:74-89` guards only against `Status == Canceled` and `ApprovedById.HasValue`. A Draft order passes both checks and is incorrectly approved.

**Fix**: Add a status guard before existing checks:

```csharp
if (order.Status != OrderStatus.Placed)
    return OrderResult.Errors.InvalidStatusTransition;
```

**Constraint**: The `ApproveOrder.cs` handler at L24 uses `Guid.TryParse` — if parsing fails, `approvedById` is `Guid.Empty`. The `Approve()` method should either accept this (current behavior — no check on approvedById) or reject `Guid.Empty`. Decide: either reject `Guid.Empty` in `Approve()` or ensure handler always provides a valid parsed ID.

### RM-003: CheckStockAvailability must populate TotalAvailable or remove it

`CheckStockAvailability.Query.cs:23-28` always returns `TotalAvailable = 0` regardless of actual availability. The field carries no information. `CheckStockAvailability.Response` defines `TotalAvailable` as a `decimal`, suggesting the intent to surface stock counts.

**Fix**: Either:
- (Option A) Populate `TotalAvailable` from `IStockAvailabilityService` by adding a method or returning it from `IsAvailableAnyLocationAsync`.
- (Option B) Remove `TotalAvailable` from `CheckStockAvailability.Response` entirely.

**Recommendation**: Option B — the query is described as "lightweight ... for UX pre-validation" and the caller (`UpdateCartItemQuantity`) only checks `IsAvailable`. Adding a count adds cost without benefit.

### RM-004: Fix unreachable IsFailure check in UpdateCartItemQuantity

`UpdateCartItemQuantity.cs:55-56` checks `stockResult.IsFailure` and returns errors. But `CheckStockAvailability.QueryHandler` (line 17-18 of `CheckStockAvailability.Query.cs`) returns `Result<Response>.Ok(new Response { ... })` even for zero/negative quantity inputs — it never returns a `Failure`. The `IsFailure` branch is dead code.

**Fix**: Remove the unreachable `IsFailure` check (lines 55-56) from `UpdateCartItemQuantity.cs`. Keep only the `IsAvailable` check (line 58-59).

### RM-005: Remove cross-module boundary violation in UpdateCartItemQuantity

`UpdateCartItemQuantity.cs:1` imports `Module.Inventory.Domain.StockLocations.StockItems` solely to reference `StockItemResult.Errors.InsufficientStock` on line 59. This is a soft boundary violation — Ordering references Inventory's domain.

**Fix**: Move the insufficient-stock error to either:
- (Option A) `CheckStockAvailability.Response` — add an `Error` property or a dedicated `WasInsufficient` flag with a descriptive message.
- (Option B) `Shared` — define `InventoryError.InsufficientStock` in `Shared.Application.Contracts.Inventory` if one exists.

**Recommendation**: Option A — extend `CheckStockAvailability.Response` with `string? InsufficientStockMessage` and return it when `IsAvailable == false`. The caller then uses `OrderResult.Errors.VariantDiscontinued`-style error (a domain-appropriate error constant from the Ordering module) instead of reaching into Inventory.

### RM-006: Guard AddToCart reservation against inactive StockLocations

`AddToCart.cs:69-72` queries `StockItem` for the best location using `CountOnHand > 0` and `OrderByDescending(si => si.CountOnHand)` but does not check `StockLocation.Active`. The obsolete `AvailabilityValidator` performed this check. If `ReserveCartStock` handler also omits the check, reservations can be placed against inactive/dead locations.

**Fix**: Before selecting `primaryLocation`, verify the associated `StockLocation` is active. Two approaches:
- Join `StockLocation` in the query and filter `si.StockLocation.Active == true`.
- Add an `Active` guard in the `ReserveCartStock` handler as defense-in-depth.

**Recommendation**: Do both — add the join here for correctness, and add the guard in `ReserveCartStock` as defense-in-depth.

### RM-007: Replace null-forgiving operator in AddToCart

`AddToCart.cs:80` uses `currentUser.UserId!` — the null-forgiving operator on a nullable property. The `Guid.TryParse` guard on line 36 ensures `userId` has a value when `IsAuthenticated` is true, so the `!` is technically safe, but a null-ref would throw at runtime if a future change bypasses the guard.

**Fix**: Replace with `currentUser.UserId ?? string.Empty`:

```csharp
var cartToken = currentUser.IsAuthenticated
    ? currentUser.UserId ?? string.Empty
    : currentUser.SessionId ?? string.Empty;
```

### RM-008: Add pagination to CartExpiryJob

`CartExpiryJob.cs:27-31` calls `ToListAsync(cancellationToken)` on the expired cart query without batching. With high traffic, millions of expired carts could materialize in a single query, causing out-of-memory errors.

**Fix**: Process in batches using `.Take(batchSize)` in a loop:

```csharp
const int batchSize = 500;
List<Order> expired;
do
{
    expired = await _dbContext.Set<Order>()
        .Where(o => o.Status == OrderStatus.Draft
            && (o.ModifiedAtUtc == null || o.ModifiedAtUtc < cutoff)
            && !o.IsDeleted)
        .Take(batchSize)
        .ToListAsync(ct);

    foreach (var cart in expired)
    {
        cart.Status = OrderStatus.Expired;
        cart.IsDeleted = true;
        cart.DeletedAtUtc = DateTimeOffset.UtcNow;
    }
    await _dbContext.SaveChangesAsync(ct);
} while (expired.Count == batchSize);
```

### RM-009: Use domain Delete() method in CartExpiryJob

`CartExpiryJob.cs:39-41` directly mutates `Status`, `IsDeleted`, and `DeletedAtUtc` instead of calling `cart.Delete(OrderConstant.Defaults.CreatedBy)`. The domain method sets `DeletedBy` in addition to the other fields, preserving the full audit trail.

**Fix**: Replace direct mutation with domain method invocation:

```csharp
foreach (var cart in expired)
{
    cart.Status = OrderStatus.Expired;
    cart.Delete(OrderConstant.Defaults.CreatedBy);
}
```

### RM-010: Simplify CancelOrder wasPlaced check

`CancelOrder.cs:45` defines `wasPlaced` as `entity.Status == OrderStatus.Placed && entity.CompletedAtUtc.HasValue`. Both `Place()` and `Finalize()` always set `CompletedAtUtc` when transitioning to `Placed`. The `HasValue` check is a defensive guard against a hypothetical future bug where `Status == Placed` is set without `CompletedAtUtc`.

**Fix**: Simplify to `entity.Status == OrderStatus.Placed`. If a future change introduces the bug this guards against, it should be caught by tests, not masked by silently skipping inventory restoration.

### RM-011: Guard Order.Empty() against Expired status

`Order.Empty()` in `Order.Method.StateMachine.cs:93-114` rejects `Placed` and `Canceled` orders but allows `Expired` orders to be emptied. An expired cart is already soft-deleted by `CartExpiryJob` — emptying it after expiry is semantically wrong and could produce confusing audit events.

**Fix**: Add `order.Status == OrderStatus.Expired` to the guard conditions:

```csharp
if (order.Status == OrderStatus.Placed || order.Status == OrderStatus.Expired)
    return OrderResult.Errors.InvalidStatusTransition;
```

### RM-012: Fix SetShippingMethod error constant

`Order.SetShippingMethod()` in `Order.Method.Checkout.cs:197-198` returns `OrderResult.Errors.NotDraftForShipAddress` when the order status is not Draft. This error constant says "shipping address" but the operation is a shipping method change — misleading for API consumers who read error codes.

**Fix**: Add `OrderResult.Errors.NotDraftForShippingMethod` error constant and use it here. Code pattern: `"Order.ShippingMethod.Update.NotDraft"`, message `"Only draft orders can have shipping method modified."`.

### RM-013: Remove redundant catch block in CreateOrderFromCart

`CreateOrderFromCart.cs:160-163` has a general `catch` block that calls `transaction.RollbackAsync(cancellationToken)` then re-throws. EF Core's `DisposeAsync` on an uncommitted transaction already performs a rollback, making this block redundant.

**Fix**: Remove lines 160-163 entirely. The `await using` declaration on the transaction variable ensures rollback on any exception that propagates out of the `try` block.

### RM-014: Drop dead StockLocation/StockItem setup from UpdateCartItemQuantity tests

`UpdateCartItemQuantity.Tests.cs:59-61` seeds a `StockLocation` into the in-memory database, and `ApplicationDbContext.AdditionalConfigurationsAssemblies` includes `typeof(StockItem).Assembly` on line 31. The handler no longer queries `StockItem` — it calls `CheckStockAvailability` via `ISender` instead. These setups are dead weight.

**Fix**: Remove lines 59-61 (StockLocation seeding), remove line 31 (`typeof(StockItem).Assembly` from `AdditionalConfigurationsAssemblies`), and remove the `using Module.Inventory.Domain.StockLocations;` import (line 2). Do this in both test methods (lines 59-61 and 102-104).

### RM-015: Delete or inline LineItem.FinalAmount()

`LineItem.FinalAmount()` in `LineItem.Method.Compute.cs:16-18` is a pure accessor — it returns `Total` unchanged. The comment explains that `Total` already includes `AdjustmentTotal`, but this doesn't justify a separate method.

**Fix**: Inline `FinalAmount()` at its single call site (if any), then remove the method. If it has no callers, delete it directly.

## 4. Interfaces & Data Contracts

### Schema changes

| Entity | Property | Change |
|---|---|---|
| `CheckStockAvailability.Response` | `TotalAvailable` | Remove (RM-003 Option B) |
| `OrderResult.Errors` | — | Add `NotDraftForShippingMethod` (RM-012) |

### Behavioral changes

| Method | Before | After |
|---|---|---|
| `Order.RecalculateTotals()` | `Total` includes shipping twice | `Total` includes shipping once (RM-001) |
| `Order.Approve()` | Accepts Draft orders | Rejects non-Placed orders (RM-002) |
| `Order.Empty()` | Allows Expired orders | Rejects Expired orders (RM-011) |
| `CheckStockAvailability.QueryHandler` | Returns `Result<Response>.Ok(...)` for all input | Unchanged (handler behavior is correct; caller fix only) |
| `UpdateCartItemQuantity.Handler` | Checks `stockResult.IsFailure` (dead code) | Removes dead `IsFailure` check (RM-004) |
| `AddToCart.cs` | No `StockLocation.Active` check | Joins `StockLocation` and filters on `Active` (RM-006) |

## 5. Acceptance Criteria

- **AC-001**: Given an order with shipping adjustments, when `RecalculateTotals()` runs, then `Total` equals `ItemTotal + ShipmentTotal + (non-shipping AdjustmentTotal)`. Shipping costs appear exactly once in the total.
- **AC-002**: Given a Draft order, when `Approve()` is called, then the method returns `InvalidStatusTransition` error.
- **AC-003**: Given an Expired cart, when `Empty()` is called, then the method returns `InvalidStatusTransition` error.
- **AC-004**: Given any input, when `CheckStockAvailability.QueryHandler.Handle()` runs, then `TotalAvailable` is either populated with the actual available count or the field no longer exists on the response.
- **AC-005**: Given the `UpdateCartItemQuantity` handler, when `CheckStockAvailability` returns `Ok(Response { IsAvailable = false })`, then the handler returns an ordering-domain error (not `StockItemResult.Errors.InsufficientStock`) and contains no `using Inventory.Domain.*` directives.
- **AC-006**: Given an inactive `StockLocation`, when `AddToCart` queries for a primary stock location, then that location is excluded from candidate locations.
- **AC-007**: Given 10,000 expired draft carts, when `CartExpiryJob.RunAsync()` executes, then memory usage stays below 100MB and all carts are expired in batches.
- **AC-008**: Given a Draft order with a shipping method, when `SetShippingMethod()` returns a failure due to non-Draft status, then the error code is `Order.ShippingMethod.Update.NotDraft`, not `Order.ShipAddress.Update.NotDraft`.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for all domain method fixes (RM-001, RM-002, RM-011, RM-012). Integration tests for the `RecalculateTotals` shipping fix to validate historical order total consistency.
- **Frameworks**: MSTest, FluentAssertions, Moq (existing project standards).
- **Test Data Management**: In-memory database via `ApplicationDbContext` with `UseInMemoryDatabase(Guid.NewGuid().ToString())`. Seed known shipping adjustments and verify `Total` computation in both directions.
- **CI/CD Integration**: All fixes must pass existing CI pipeline (`dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests`).
- **Coverage Requirements**: Each RM must have at least one unit test exercising the fixed behavior. Existing tests that relied on incorrect behavior (e.g., test asserting old `Total` value) must be updated.
- **Performance Testing**: For RM-008 (CartExpiryJob batching): seed 5,000 expired carts in an integration test and verify all are expired within a single execution.

### Specific test additions

| RM | Test | Description |
|---|---|---|
| RM-001 | `RecalculateTotals_WithShippingAdjustment_DoesNotCountShippingTwice` | Create order with one shipping adjustment, verify `Total = ItemTotal + shippingAmount + otherAdjustments` |
| RM-002 | `Approve_DraftOrder_ReturnsInvalidStatusTransition` | Draft order, call `Approve()`, assert failure |
| RM-002 | `Approve_PlacedOrder_Succeeds` | Placed order, call `Approve()`, assert success |
| RM-011 | `Empty_ExpiredOrder_ReturnsInvalidStatusTransition` | Expired order, call `Empty()`, assert failure |
| RM-006 | `AddToCart_FindsPrimaryLocation_ExcludesInactiveLocations` | Inactive location with stock, active location without stock — assert no reservation or empty-cart behavior |

## 7. Rationale & Context

### RM-001 double-counting rationale

The invariant comment at the top of `Order.Method.Computation.cs` reads: `ItemTotal + ShipmentTotal + AdjustmentTotal`. The implementation sums all eligible adjustments into `AdjustmentTotal` first, then sums shipping adjustments into `ShipmentTotal`, then adds both. The semantic distinction between `ShipmentTotal` (shipping-only) and `AdjustmentTotal` (non-shipping adjustments) is embedded in the property names but violated by the computation. The fix aligns implementation with semantics.

**Impact**: This is the highest-severity defect. Every order with shipping costs has an inflated `Total`. Payment validation (RM `ValidatePayment`) uses `Total`, so customers are charged double shipping. No reports of this yet likely because shipping methods are rarely configured in dev/test environments.

### RM-002 approve Draft rationale

The admin feature `ApproveOrder` is designed for warehouse workflow — an admin approves a placed order before fulfillment. Approving a Draft order (an empty or partially-filled cart) has no valid business meaning. The domain guard protects against accidental or API-misuse approval of carts.

### RM-004 dead code rationale

The `stockResult.IsFailure` check in `UpdateCartItemQuantity` was written defensively assuming `CheckStockAvailability` could return failures. But `CheckStockAvailability.QueryHandler` wraps all outcomes in `Result<Response>.Ok(...)` — even invalid input. Removing dead code reduces confusion about what errors the handler can actually produce.

### RM-005 boundary violation rationale

The codebase rule "Modules never reference each other" applies to domain types. While the feature layer may reference other modules via MediatR contracts, directly importing another module's domain types and error constants creates a hard compile-time dependency that defeats the purpose of modular isolation. Using MediatR response types or shared contracts preserves the boundary.

## 8. Dependencies & External Integrations

### Cross-Module Dependencies
- **MOD-001**: `Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability` — Ordering calls this via MediatR. RM-003 and RM-005 modify this feature's `Response` type.
- **MOD-002**: `Module.Inventory.Features.Storefront.CartReservations.Reserve.ReserveCartStock` — Ordering calls this via MediatR in `AddToCart`. RM-006 recommends a defense-in-depth guard in this handler.

### Internal Domain Dependencies
- **DOM-001**: `Order.RecalculateTotals()` — called by 8+ handlers (`Place`, `Finalize`, `Empty`, `AddLineItem`, `RemoveLineItem`, `ReplaceShippingAdjustment`, `Merge`, `UpdateCheckout`). RM-001 changes the computation; all callers are affected but required to pass updated assertions.

### Test Infrastructure
- **TST-001**: `UpdateCartItemQuantity.Tests` — In-memory database with `AdditionalConfigurationsAssemblies` including `StockItem` and `StockLocation`. RM-014 removes these.

## 9. Examples & Edge Cases

### RM-001: Double-counting verification

```csharp
// Order has: ItemTotal=100, Shipping adjustment=10, Tax adjustment=5
// Before fix:
//   AdjustmentTotal = 0 (line-item) + (10 + 5) = 15  ← includes shipping!
//   ShipmentTotal = 10
//   Total = 100 + 10 + 15 = 125                        ← shipping counted twice

// After fix:
//   AdjustmentTotal = 0 (line-item) + 5 = 5           ← excludes shipping
//   ShipmentTotal = 10
//   Total = 100 + 10 + 5 = 115                        ← correct
```

### RM-006: Inactive location edge case

```csharp
// Location A: Active=true, CountOnHand=0
// Location B: Active=false, CountOnHand=100
// Before fix: B is selected as primary location → reservation against dead location
// After fix: neither location qualifies → no reservation → item added to cart without stock hold
```

## 10. Validation Criteria

- **VC-001**: `dotnet build` passes with warnings-as-errors — no new compiler warnings from boundary changes (RM-005)
- **VC-002**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"` — all existing Ordering unit tests pass after fix, plus new tests added per §6 table
- **VC-003**: No `using Module.Inventory.Domain.*` directives remain in `service/Api/src/Module/Ordering/` (RM-005)
- **VC-004**: `CartExpiryJob.RunAsync()` processes at most 500 entities per `SaveChangesAsync` call (RM-008)
- **VC-005**: `CheckStockAvailability.Response.TotalAvailable` is either always populated with correct value or removed from the type (RM-003)

## 11. Related Specifications / Further Reading

- [spec-design-payment-bugfixes.md](./spec-design-payment-bugfixes.md) — sibling remediation spec for Payment module
- `docs/codebase/ARCHITECTURE.md` — modular monolith layer boundaries and cross-module communication rules
- `docs/codebase/CONVENTIONS.md` — naming, error constants, and domain method patterns
- `.harness/enforcement.yml` — import/using rules and boundary enforcement
