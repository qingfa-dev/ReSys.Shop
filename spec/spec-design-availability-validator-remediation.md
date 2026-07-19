---
title: AvailabilityValidator Remediation — Resolve Cross-Module Boundary Violation and Reservation-Aware Stock Checks
version: 1.0
date_created: 2026-07-20
owner: Platform Team
tags: design, remediation, inventory, ordering, module-isolation
---

# Introduction

Code review of `AvailabilityValidator` found three overlapping problems: (a) the class is a static utility in `Module.Inventory.Domain.Stock` called directly from `Module.Ordering` — violating the module boundary rule, (b) its `IsAvailable` overload ignores active `StockReservation` rows — returning false positives when stock appears available but is fully reserved (RISK-006), and (c) the correct reservation-aware availability logic is duplicated across four locations (two services, one feature handler, and `StockAvailabilityService`). This spec prescribes deletion, realignment, and deduplication.

## 1. Purpose & Scope

**Purpose**: Eliminate the cross-module boundary violation by removing `AvailabilityValidator` and its static call sites in Ordering, replacing them with ISender-based queries that use reservation-aware availability logic already present in the Inventory module.

**Scope**:
- `service/Api/src/Module/Inventory/Domain/Stock/AvailabilityValidator.cs` — delete
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — replace static call
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` — replace static call
- `service/Api/src/Module/Inventory/Services/StockAvailabilityService.cs` — used as backend for new query
- `service/Api/src/Module/Inventory/Features/Storefront/` — new MediatR query feature

**Out of scope**: Deduplication of reservation-aware logic across `StockReservationService`, `CartReservationService`, and `ReserveCartStock` handler (all three independently inline the same `SumAsync` pattern — addressed in a follow-up spec). Adding cart reservation updates to `UpdateCartItemQuantity` (the current handler never touches stock reservations on quantity change).

## 2. Definitions

| Term | Definition |
|---|---|
| Module boundary | Rule per `.harness/enforcement.yml:83-90` — modules must not cross-reference; cross-module communication via `ISender` only. |
| Reservation-aware availability | `CountOnHand − sum(active Reserved reservations with ExpiresAtUtc > now)`. Raw on-hand produces false positives. |
| Serializable pre-check | The `ReserveCartStock` handler wraps its availability check in `IsolationLevel.Serializable` — this is the authoritative guard against oversell. |
| Lightweight availability query | A read-only (no transaction, slightly stale tolerated) check suitable for UX pre-validation. |

## 3. Requirements, Constraints & Guidelines

### 🔴 Module Boundary

- **BND-001**: `AvailabilityValidator` must be deleted. Any Ordering module handler that needs stock availability data must query Inventory via `ISender`, not via `using Module.Inventory.Domain.Stock`.
- **BND-002**: `AddToCart.cs` must not `using Module.Inventory.Domain.Stock` (or any other Inventory namespace except the MediatR feature namespace for the command it already sends).

### 🔴 Bug Fixes

- **BUG-001**: The `IsAvailable` overload (reservation-unaware) must be removed. No caller may rely on raw `CountOnHand` sums when active reservations exist.

### 🟡 Design Decisions

- **DES-001**: `AddToCart` pre-check (line 75) is redundant — the `ReserveCartStock` handler (line 85–105) already performs a reservation-aware availability check inside a serializable transaction. Remove the pre-check and rely on `ReserveCartStock`'s result. If the reserve fails with `InsufficientStock`, the cart is not persisted (no `SaveChangesAsync` has been called yet).
- **DES-002**: `UpdateCartItemQuantity` pre-check (line 50) has no corresponding reserve call. Removing it means no stock validation at all at quantity-change time. Replacement: create a `CheckStockAvailability` MediatR query in Inventory's `Features/Storefront/StockAvailability/Check/` that wraps `IStockAvailabilityService.IsAvailableAnyLocationAsync`, and call it via `ISender` from Ordering.
- **DES-003**: The new `CheckStockAvailability` query tolerates slightly stale reads (no serializable isolation) — its purpose is UX feedback, not concurrency safety. The authoritative check remains inside `ReserveCartStock`'s serializable transaction.

## 4. Interfaces & Data Contracts

### New Inventory Feature: `CheckStockAvailability`

```
Module/Inventory/
  Features/Storefront/
    StockAvailability/
      Check/
        CheckStockAvailability.Query.cs
        CheckStockAvailability.Handler.cs
        CheckStockAvailability.Response.cs
```

**Query**: `CheckStockAvailability.Query(Guid VariantId, int Quantity) : IQuery<CheckStockAvailability.Response>`

**Response**:
```csharp
public sealed record CheckStockAvailability.Response : StockCheckResponse
{
    public bool IsAvailable { get; init; }
    public int TotalAvailable { get; init; }
}
```

**Base** (in `Features/Storefront/StockAvailability/Shared/Models/StockCheckResponse.cs`):
```csharp
public abstract record StockCheckResponse;
```

**Handler logic**: delegates to `IStockAvailabilityService.IsAvailableAnyLocationAsync(variantId, quantity, ct)` and returns `TotalAvailable` from `StockAvailabilityCalculator` or a simple `CountOnHand − reserved` query.

### Deleted Artifact

- `Module/Inventory/Domain/Stock/AvailabilityValidator.cs` — all three methods (`IsAvailable`, `IsAvailableWithReservations`, `TotalAvailable`) removed. `TotalAvailable` logic is already available via `IStockAvailabilityService` or can be inlined.

### Modified Call Sites

**AddToCart.cs (Ordering → Inventory)**:
- Remove `using Module.Inventory.Domain.Stock;` (line 4)
- Remove `using Module.Inventory.Domain.StockLocations.StockItems;` (line 6 — check if still needed for `Set<StockItem>()` call at line 70)
- Remove lines 69–76 (stock items load + static pre-check)
- Remove lines 79–82 (primary location computation) — repurpose `primaryLocation` from `StockAvailabilityCalculator` or select directly from the first location with `CountOnHand > 0`
- Keep lines 84–105 (`ReserveCartStock` send) — handler already validates availability

**UpdateCartItemQuantity.cs (Ordering → Inventory)**:
- Remove `using Module.Inventory.Domain.Stock;` (line 1)
- Remove lines 45–51 (stock validation block)
- Add `ISender sender` to `CommandHandler` constructor
- Add ISender call to `new CheckStockAvailability.Query(lineItem.VariantId, command.Request.Quantity)` before the quantity update

## 5. Acceptance Criteria

- **AC-BND-001**: `AvailabilityValidator.cs` no longer exists in the codebase.
- **AC-BND-002**: Grep of `Module/Ordering/` for `using Module.Inventory.Domain.Stock` returns zero results.
- **AC-BUG-001**: `AddToCart` handler no longer calls any `AvailabilityValidator` method. A variant with `CountOnHand = 3`, `activeReserved = 3` (0 available) correctly gets `InsufficientStock` from the `ReserveCartStock` handler's serializable check.
- **AC-DES-001**: Removing `AddToCart`'s pre-check does not break existing behavior — the `ReserveCartStock` handler is the authoritative gate; refer to existing test `AddToCart` tests for regression.
- **AC-DES-002**: `UpdateCartItemQuantity` handler calls `CheckStockAvailability.Query` via `ISender`. Test: variant `CountOnHand = 5`, `activeReserved = 5` → query returns `IsAvailable = false` → `UpdateCartItemQuantity` returns `InsufficientStock`.
- **AC-DES-003**: The new `CheckStockAvailability` feature follows vertical-slice conventions per `spec-design-feature-conventions-remediation.md` (Query → Handler → Response, base response class in `Shared/Models/`).

## 6. Test Automation Strategy

- **Unit tests**: Extend `Module.UnitTests`:
  - `Inventory/Features/Storefront/StockAvailability/Check/CheckStockAvailabilityTests.cs` — test the new query handler (InMemory provider, mock data)
  - `Ordering/Features/Storefront/Cart/AddItem/AddToCartTests.cs` — verify that `AddToCart` returns `InsufficientStock` when `ReserveCartStock` handler does (the test mocks `ISender` to return the error)
  - `Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantityTests.cs` — verify new stock-check flow
- **No integration tests** — the lightweight check tolerates stale reads; the serializable check is already covered by the `ReserveCartStock` integration path.
- **Run**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Inventory|Ordering"` must pass with zero failures.

## 7. Rationale & Context

### Why delete instead of fix

`AvailabilityValidator` has three methods. Two (`IsAvailable`, `IsAvailableWithReservations`) perform the same operation as `IStockAvailabilityService` but (a) are static (not mockable/testable as a unit), (b) accept in-memory `IEnumerable<StockItem>` (forcing callers to materialize stock data in the handler, duplicating what a proper query would do), and (c) live in the domain layer — the wrong layer for a database-backed availability check (domain methods should operate on single aggregates, not cross-aggregate queries).

`TotalAvailable` is a one-line LINQ sum already present inside `StockAvailabilityService`.

### Why not use `IStockAvailabilityService` directly in Ordering

Injecting `IStockAvailabilityService` into Ordering handlers would be another form of cross-module boundary violation (DI reference across module boundaries). ISender dispatch is the mandated path per `.harness/enforcement.yml`.

### Why the pre-check in AddToCart is safe to remove

`AddToCart.Handle` creates/loads the cart entity (tracker-only), calls `ReserveCartStock` via ISender (which opens its own serializable transaction and commits independently), then saves cart + line items only after the reserve succeeds. If the reserve fails, no cart data was persisted — the transaction in `ReserveCartStock` is self-contained and does not affect the outer DbContext's transaction state.

### Why UpdateCartItemQuantity needs the new query

Unlike `AddToCart`, `UpdateCartItemQuantity` has no matching `ReserveCartStock` call for the delta. Without any check, a user could increase cart quantity beyond available stock with no immediate feedback — wrong info until checkout. The lightweight `CheckStockAvailability` query gives immediate UX feedback (false positives are acceptable for a slightly stale read; false negatives are the only concern, and these are impossible — a read never sees less available stock than reality).

## 8. Dependencies & External Integrations

### Cross-Module Dependencies

- **SVC-001**: `IStockAvailabilityService` (Inventory service) — backend for the new `CheckStockAvailability` query handler. No contract change.
- **SVC-002**: `ReserveCartStock` handler (Inventory feature) — called from `AddToCart` via ISender. No change.
- **SVC-003**: `ISender` (MediatR, Shared assembly) — Ordering handlers inject `ISender` to dispatch to Inventory feature handlers.

### Technology Platform Dependencies

- **PLT-001**: EF Core InMemory provider — used in unit tests; `AvailabilityValidator` was static and testable without it, but replacement handlers require InMemory test setup.

## 9. Examples & Edge Cases

### Before/After: AddToCart stock validation

```csharp
// Before (broken — module boundary violation + reservation-unaware)
using Module.Inventory.Domain.Stock;

var stockItems = await dbContext.Set<StockItem>()
    .Include(x => x.StockLocation)
    .Where(x => x.VariantId == request.VariantId)
    .ToListAsync(cancellationToken);

if (!AvailabilityValidator.IsAvailable(stockItems, request.Quantity))
    return StockItemResult.Errors.InsufficientStock;

var primaryLocation = stockItems
    .Where(si => si.CountOnHand > 0)
    .OrderByDescending(si => si.CountOnHand)
    .FirstOrDefault();

// After (correct — MediatR dispatch to authoritative check)
// No pre-check here. ReserveCartStock handler validates inside serializable tx.
var stockItem = await dbContext.Set<StockItem>()
    .Where(si => si.VariantId == request.VariantId && si.CountOnHand > 0)
    .OrderByDescending(si => si.CountOnHand)
    .FirstOrDefaultAsync(cancellationToken);

if (stockItem is not null)
{
    var reserveResult = await sender.Send(
        new ReserveCartStock.Command(new ReserveCartStock.Request { ... }),
        cancellationToken);
    if (reserveResult.IsFailure)
        return reserveResult.Errors;  // <— authoritative check
}
```

### Before/After: UpdateCartItemQuantity stock validation

```csharp
// Before (broken — module boundary violation + reservation-unaware)
using Module.Inventory.Domain.Stock;

var stockItems = await dbContext.Set<StockItem>()
    .Where(x => x.VariantId == lineItem.VariantId)
    .ToListAsync(cancellationToken);

if (!AvailabilityValidator.IsAvailable(stockItems, command.Request.Quantity))
    return StockItemResult.Errors.InsufficientStock;

// After (correct — MediatR query to reservation-aware service)
var stockResult = await sender.Send(
    new CheckStockAvailability.Query(lineItem.VariantId, command.Request.Quantity),
    cancellationToken);

if (!stockResult.Value.IsAvailable)
    return StockItemResult.Errors.InsufficientStock;
```

### Edge Case: quantity zero

`CheckStockAvailability.Query` with `Quantity = 0` returns `IsAvailable = true`. This is the same behavior as the deleted `AvailabilityValidator.IsAvailable`.

### Edge Case: variant with no stock items

`IStockAvailabilityService.IsAvailableAnyLocationAsync` returns `false` when no `StockItem` exists for the variant. The `CheckStockAvailability.Query` handler propagates this correctly.

### Edge Case: backorderable locations

`AvailabilityValidator.IsAvailable` considered `Backorderable` a fallback — if no location had raw on-hand, a backorderable location made it return `true`. `IStockAvailabilityService` does NOT consider backorderability. This is a deliberate simplification: backorder decisions belong at checkout time, not at add-to-cart or quantity-update time.

## 10. Validation Criteria

- **VC-001**: `dotnet build` passes with warnings-as-errors (no new warnings).
- **VC-002**: All existing inventory unit tests pass: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Inventory"`.
- **VC-003**: All existing ordering unit tests pass: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`.
- **VC-004**: `AvailabilityValidator.cs` file is deleted and no `using Module.Inventory.Domain.Stock` remains in any `Module/Ordering/` file.
- **VC-005**: New `CheckStockAvailability` feature passes its unit tests.
- **VC-006**: No EF Core migration is required (no schema change).

## 11. Related Specifications / Further Reading

- [spec-design-inventory-bugfixes.md](spec-design-inventory-bugfixes.md) — RISK-006 and RISK-002 (original findings)
- [spec-design-feature-conventions-remediation.md](spec-design-feature-conventions-remediation.md) — Feature Command/Query/Request/Response conventions the new query must follow
- `.harness/enforcement.yml` — Import boundary check rules (lines 81–93)
- `.harness/principles.yml` — Module isolation, vertical slice isolation principles
