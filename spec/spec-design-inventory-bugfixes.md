---
title: Inventory System Bugfixes — Reservation, Stock Reconciliation, and Concurrency
version: 1.0
date_created: 2026-07-19
owner: Platform Team
tags: design, bugfix, inventory, reservation, concurrency
---

# Introduction

Code review of the 286-file Inventory module (`Service/Api/src/Module/Inventory/`) across domain entities, services, feature handlers, persistence, and backgrounds identified 20 findings: 5 critical bugs causing stock leaks or oversell, 10 race/risk conditions, and 5 nits. This spec captures each finding as a requirement with an acceptance criterion and fix target.

## 1. Purpose & Scope

**Purpose**: Resolve all identified bugs and risks in the inventory reservation lifecycle, stock reconciliation paths, concurrency guards, and expiry mechanisms.

**Scope**: `Module/Inventory/` plus cross-module consumers (`Module/Ordering/Features/Shared/Services/OrderInventoryService.cs`, `Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs`). Shared contract `IStockQuantityService` in-scope as the integration boundary.

**Out of scope**: Vue frontends, benchmarks, embedding service, CI/CD changes, adding new features beyond bugfixes.

## 2. Definitions

| Term | Definition |
|---|---|
| Reservation lifecycle | `Reserved → Fulfilled | Released | Expired`. Each transition must restore or consume stock exactly once. |
| Stock reconciliation | The act of restoring `CountOnHand` when a reservation is released or expired. Each reserved quantity must be restored exactly once. |
| Available stock | `CountOnHand − activeReserved` (where active = `State == Reserved && ExpiresAtUtc > now`). Not raw `CountOnHand`. |
| Backorder | A reservation that could not be immediately stocked. Fulfilled later in FIFO order when restock arrives. |
| Serializable transaction | `IsolationLevel.Serializable` used to prevent phantom reads and oversell in concurrent reserve operations. |
| Slightly stale read | Read queries (availability checks, summary) tolerate uncommitted data from concurrent operations — they do not require serializable isolation. |

## 3. Requirements, Constraints & Guidelines

### 🔴 Critical Bugs

**BUG-001: `CartReservationService.ReleaseCartReservationsAsync` — missing `SaveChangesAsync`**
`CartReservationService.cs:87` — The method modifies reservation states and restores stock via `stockItem.CountOnHand += r.Quantity` inside a `foreach` loop, then exits without calling `await _dbContext.SaveChangesAsync(cancellationToken)`. All mutations are tracked in-memory but never persisted. This is a silent no-op.
- Fix: Add `await _dbContext.SaveChangesAsync(cancellationToken)` after the loop, mirroring `StockReservationService.ReleaseReservationsAsync:93`.

**BUG-002: `ReleaseCartReservation` handler never restores `CountOnHand`**
`ReleaseCartReservation.cs:25` — The handler mutates `reservation.State = ReservationState.Released` but never increments the stock item's `CountOnHand`. The released quantity is permanently deducted from available inventory.
- Fix: After releasing, load the associated `StockItem` and add `reservation.Quantity` back to `CountOnHand`, matching the pattern in `StockReservationService.ReleaseReservationsAsync:88`.

**BUG-003: `StockQuantityService.DecrementStockAsync` ignores active reservations**
`StockQuantityService.cs:47` — The guard `stockItem.CountOnHand < quantity` uses raw on-hand count. If 10 on-hand with 3 active reserved, a decrement of 8 succeeds (should be rejected — only 7 actually available).
- Fix: Compute available stock as `CountOnHand - activeReserved` before the guard check. Apply Serializable isolation.

**BUG-004: Double-restore on repeated release of same reservation**
`StockReservationService.cs:78` / `ReleaseReservationsAsync`, `ExpireReservationsAsync`, `ExpireReservationsAndRestoreStockAsync` (lines 78, 113, 170) — Each method unconditionally calls `stockItem.CountOnHand += r.Quantity` without checking if the reservation was already released/expired. If called twice (retry, racing expiry sweeps), stock is double-restored.
- Fix: Add a guard `if (r.State == ReservationState.Reserved)` before restoring stock. Set state to `Released`/`Expired` first, then restore.

**BUG-005: `CartReservationService.GetReservationsForCartAsync` null-forgiving on `ExpiresAtUtc`**
`CartReservationService.cs:104` — `r.ExpiresAtUtc!.Value` uses `!` null-forgiving operator. If the reservation was created without `ExpiresAtUtc` (e.g. seed data), this throws `NullReferenceException`.
- Fix: Add `&& r.ExpiresAtUtc != null` to the EF `Where` filter, making the `!` safe.

### 🟡 Race / Risk Conditions

**RISK-001: `StockReservationService.ReserveAsync` lacks transaction**
`StockReservationService.cs:36-62` — No transaction isolation. Between reading the stock item and inserting the reservation, another concurrent request can create a conflicting reservation. The `ReserveCartStock` handler already uses Serializable isolation; the service-level method should too.
- Fix: Wrap the entire reserve in `await using var tx = await _dbContext.BeginTransactionAsync(IsolationLevel.Serializable, ct)`.

**RISK-002: `ReleaseCartReservation` handler — no concurrency guard**
`ReleaseCartReservation.cs:16-29` — The handler loads a reservation by ID and mutates state. No `RowVersion` check or transaction. Two concurrent releases both succeed silently.
- Fix: Either add a `WHERE RowVersion = @rv` check or wrap in serializable tx. The reservation entity currently lacks a `RowVersion` column — add one.

**RISK-003: `BulkAdjustStockItems` bypasses domain validation**
`BulkAdjustStockItems.cs:38` — Directly mutates `entity.CountOnHand += item.Quantity` without validating the result is ≥ 0. A negative adjustment can push stock below zero.
- Fix: Call `StockItemMethod.AdjustCountOnHand(entity, item.Quantity, reason)` instead of direct mutation. The domain method enforces `newCount >= 0`.

**RISK-004: `StockRestockService.FulfillBackordersInternalAsync` matches any active reservation**
`StockRestockService.cs:89-93` — The query matches all `Reserved` reservations for the variant/location with FIFO ordering. This includes regular pending orders, not just backorders. Should only fulfill reservations explicitly flagged as backorders.
- Fix: Add a backorder indicator to `StockReservation` (e.g., `IsBackorder` flag or `Reason == "backorder"`) and filter the query.

**RISK-005: Dual expiry sweep — `ReservationExpiryService` + `ReservationExpiryJob`**
`ReservationExpiryService.cs:26` (BackgroundService, sweep every 60s) and `ReservationExpiryJob.Scheduler.cs:18` (Hangfire recurring job) both call `ExpireReservationsAndRestoreStockAsync`. This doubles the database work and races on the same rows.
- Fix: Eliminate one mechanism. Recommendation: remove `ReservationExpiryService` and keep the Hangfire job (already wired with configurable cron).

**RISK-006: `AvailabilityValidator.IsAvailable` ignores reservations**
`AvailabilityValidator.cs:19-22` — Sums `CountOnHand` across active locations but ignores active reservations. Returns `true` when raw stock appears sufficient but reserved stock makes it unavailable.
- Fix: Either subtract active reservations or delegate to `StockAvailabilityCalculator.GetForVariantAsync`.

**RISK-007: `ReservationExpiryJob.Scheduler` — `CancellationToken.None`**
`ReservationExpiryJob.Scheduler.cs:20` — The Hangfire job registration passes `CancellationToken.None`. On host shutdown, the job cannot be gracefully cancelled.
- Fix: Inject `IHostApplicationLifetime` and pass `lifetime.ApplicationStopping` as the token.

**RISK-008: `StockTransfer` entity — no optimistic concurrency token**
`StockTransfer.cs` and `StockTransferConfiguration.cs` — The entity lacks a `RowVersion` column. Two admins can simultaneously transfer/receive the same transfer draft.
- Fix: Add `uint RowVersion` property and `.IsRowVersion()` in the configuration.

**RISK-009: `StockSummaryService` — low-stock threshold ignores reservations**
`StockSummaryService.cs:60` — `IsLowStock = si.CountOnHand <= si.StockLocation.LowStockThreshold` uses raw on-hand, not available stock. A variant with 100 on-hand and 98 reserved (2 available) with threshold 5 is not flagged.
- Fix: Use `available` (already computed at line 52) instead of `si.CountOnHand`.

**RISK-010: `StockReservationConfiguration` — missing query indexes**
`StockReservationConfiguration.cs` — No index on `(OrderId, State)` or `(CartToken, State)`. Both `ReleaseReservationsAsync` and `ReleaseCartReservationsAsync` scan by these columns.
- Fix: Add composite indexes: `HasIndex(r => new { r.OrderId, r.State })` and `HasIndex(r => new { r.CartToken, r.State })`.

### 🔵 Nits

**NIT-001: `ReleaseCartReservation` bypasses `StockReservationMethod.Release()`**
`ReleaseCartReservation.cs:22-27` — The handler mutates `State` and `ExpiresAtUtc` directly instead of calling `reservation.Release()`. The domain method provides expiry validation.
- Fix: Call `var releaseResult = reservation.Release(); if (releaseResult.IsFailure) return releaseResult;`.

**NIT-002: Duplicate availability-calculation logic**
`CartReservationService.cs:32-48`, `StockReservationService.cs:36-52`, `StockAvailabilityService.cs:34-50` — All three contain identical stock-item lookup + reserved-sum + available-check logic.
- Fix: Extract into a shared private helper `async Task<int> GetAvailableStockAsync(Guid variantId, Guid stockLocationId, CancellationToken ct)`.

**NIT-003: `CartReservationService.ReleaseCartReservationsAsync` inconsistent with order equivalent**
`CartReservationService.cs:64-87` lacks the `SaveChangesAsync` and `Result` wrappers that `StockReservationService.ReleaseReservationsAsync:68-94` has.
- Fix: After fixing BUG-001 and BUG-004, harmonize both methods into a shared private or base-class helper.

**NIT-004: `GetAvailability.cs` — sequential N+1 for out-of-stock variants**
`GetAvailability.cs:95-97` — For each out-of-stock variant, calls `calculator.GetForVariantAsync(v.Id, ct)` synchronously in a loop. For products with many variants, this is N+1.
- Fix: Batch-fetch all variant snapshots upfront with a single multi-variant query.

**NIT-005: `ReserveCartStock` — cart reservation has null `OrderId`**
`ReserveCartStock.cs:61` — Passes `null` for the `orderId` parameter. When the cart converts to an order, `DecrementStockAsync` matches reservations by `OrderId` and will not find this cart reservation.
- Fix: After order creation, run an `UPDATE` to patch `OrderId` on any released cart reservations, or introduce a `ReservationId` fulfillment path.

## 4. Interfaces & Data Contracts

### Modified Entities

**StockReservation** — add fields:
| Field | Type | Purpose |
|---|---|---|
| `RowVersion` | `uint` | Optimistic concurrency token |
| `IsBackorder` | `bool` | Distinguishes backorder reservations from standard pending orders |

**StockTransfer** — add field:
| Field | Type | Purpose |
|---|---|---|
| `RowVersion` | `uint` | Optimistic concurrency token |

### Modified Service Contracts

None. All changes are internal to existing interfaces. `IStockReservationService` retains its current shape.

### Modified DB Schema

| Table | Change | Rationale |
|---|---|---|
| `inventory.stock_reservations` | New index `(order_id, state)` | Speeds up release-by-order queries |
| `inventory.stock_reservations` | New index `(cart_token, state)` | Speeds up release-by-cart queries |
| `inventory.stock_reservations` | New column `is_backorder` (bool, default false) | Separates backorder reservations from pending orders |
| `inventory.stock_transfers` | New column `row_version` (xmin) | Enables optimistic concurrency |

All new columns require an EF Core migration.

## 5. Acceptance Criteria

**AC-BUG-001**: Given active cart reservations exist, When `ReleaseCartReservationsAsync(cartToken)` completes, Then all reservations are in `Released` state AND stock items have restored `CountOnHand` in the database (verified by a second query after `SaveChangesAsync`).

**AC-BUG-002**: Given a reservation with `State == Reserved` and a stock item with `CountOnHand = 5`, When `ReleaseCartReservation` is called, Then `CountOnHand` becomes `5 + reservation.Quantity` (persisted).

**AC-BUG-003**: Given `CountOnHand = 10` with `activeReserved = 3`, When `DecrementStockAsync(quantity: 8)` is called, Then the operation returns `InsufficientStock` error.

**AC-BUG-004**: Given a reservation is already `Released`, When `ReleaseReservationsAsync` or `ExpireReservationsAsync` is called again, Then `CountOnHand` is NOT incremented a second time.

**AC-BUG-005**: Given a `StockReservation` with `ExpiresAtUtc = null`, When `GetReservationsForCartAsync` is called for its cart token, Then no `NullReferenceException` is thrown and the reservation is excluded from results.

**AC-RISK-001**: Given two concurrent `ReserveAsync` calls for the same variant/location with insufficient combined stock, Then exactly one succeeds and one returns `InsufficientStock`.

**AC-RISK-002**: Given two concurrent `ReleaseCartReservation` calls for the same reservation ID, Then exactly one succeeds (or both succeed idempotently without double-stock-restore).

**AC-RISK-003**: Given `CountOnHand = 3`, When `BulkAdjustStockItems` processes `Quantity = -5` for this item, Then the operation returns `NegativeCountOnHand` error and stock is unchanged.

**AC-RISK-004**: Given a standard pending-order reservation and a backorder reservation for the same variant, When a restock arrives, Then only the backorder reservation is fulfilled; the pending order reservation is untouched.

**AC-RISK-005**: Given the system is running, Then exactly one expiry sweep mechanism is active (not two).

**AC-RISK-006**: Given `CountOnHand = 3` with `activeReserved = 2`, When `AvailabilityValidator.IsAvailable(quantity: 2)` is called, Then the result is `false`.

**AC-RISK-007**: Given a host shutdown signal, When the expiry job is executing, Then it cancels gracefully within the configured timeout.

**AC-RISK-008**: Given two concurrent `TransferStockTransfer` commands for the same draft, Then exactly one succeeds; the other receives a concurrency conflict error.

**AC-RISK-009**: Given `CountOnHand = 100`, `activeReserved = 98`, and `LowStockThreshold = 5`, When `GetStockSummaryAsync` runs, Then `IsLowStock = true` for that location.

**AC-RISK-010**: Given a release-by-cart query runs, Then the database uses an index seek (not a sequential scan) on `(cart_token, state)`.

## 6. Test Automation Strategy

- **Unit tests**: Extend `Module.UnitTests` to cover each bugfix. New tests in:
  - `Inventory/Services/CartReservationServiceTests.cs` — BUG-001, BUG-005
  - `Inventory/Services/StockReservationServiceTests.cs` — BUG-004, RISK-001
  - `Inventory/Services/StockQuantityServiceTests.cs` — BUG-003
  - `Inventory/Domain/StockReservations/` — BUG-004 (domain method guards)
  - `Inventory/Features/Storefront/CartReservations/Release/` — BUG-002, RISK-002
  - `Inventory/Features/Admin/StockItems/BulkAdjust/` — RISK-003
- **Integration tests**: Concurrency tests (RISK-001, RISK-002, RISK-008) require parallel execution against Testcontainers PostgreSQL.
- **Coverage**: Each bugfix must have ≥ 1 unit test. Concurrency fixes require ≥ 1 integration test.
- **Run**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Inventory"` must pass with zero failures.

## 7. Rationale & Context

The inventory system implements a reservation-based model where stock is reserved (deducted from available), then fulfilled (permanently consumed) or released (restored). The critical bugs fall into two patterns:

1. **Missing persistence**: Methods that mutate tracked entities without calling `SaveChangesAsync` (BUG-001, partially BUG-002 if the stock restore path also lacks save).
2. **Missing reconciliation**: State transitions that don't update `CountOnHand` (BUG-002) or that double-count (BUG-003 double-deduction via ignoring reservations, BUG-004 double-restoration via missing idempotency guard).

The risk findings fall into:
- **Race windows**: Methods without serializable transactions (RISK-001) or optimistic concurrency (RISK-002, RISK-008).
- **Incorrect queries**: Broad filters that match unintended data (RISK-004, RISK-006, RISK-009).
- **Architectural duplication**: Two expiry mechanisms (RISK-005).

## 8. Dependencies & External Integrations

### Technology Platform Dependencies

- **PLT-001**: PostgreSQL — Serializable isolation required for reserve and decrement operations. `SELECT ... FOR UPDATE` (`xmin` / `xid` via `uint RowVersion`) for optimistic concurrency.
- **PLT-002**: EF Core 10 — migrations needed for new columns (`is_backorder`, `row_version`) and indexes.
- **PLT-003**: Hangfire — recurring job scheduling for expiry sweep (one mechanism, not two).

### Cross-Module Dependencies

- **SVC-001**: `IStockQuantityService` (Shared contract) — `DecrementStockAsync` signature unchanged but behavior changes to respect active reservations. Callers in Ordering module must be aware that `InsufficientStock` errors may increase.
- **SVC-002**: `IStockAvailabilityCalculator` (Inventory service) — consumed by Catalog's `GetAvailability` handler. No contract change.

## 9. Examples & Edge Cases

### BUG-001: Missing SaveChangesAsync

```csharp
// Before (broken — state and stock mutated in tracker, never persisted)
public async Task ReleaseCartReservationsAsync(string cartToken, CancellationToken ct)
{
    var reservations = await _dbContext.Set<StockReservation>()
        .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved)
        .ToListAsync(ct);
    foreach (var r in reservations)
    {
        r.State = ReservationState.Released;
        r.ModifiedAtUtc = DateTimeOffset.UtcNow;
        if (r.StockLocationId is not null)
        {
            var stockItem = await _dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, ct);
            if (stockItem is not null)
                stockItem.CountOnHand += r.Quantity;
        }
    }
    // MISSING: await _dbContext.SaveChangesAsync(ct);
}

// After
public async Task ReleaseCartReservationsAsync(string cartToken, CancellationToken ct)
{
    var reservations = await _dbContext.Set<StockReservation>()
        .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved)
        .ToListAsync(ct);
    foreach (var r in reservations)
    {
        r.State = ReservationState.Released;
        r.ModifiedAtUtc = DateTimeOffset.UtcNow;
        if (r.StockLocationId is not null)
        {
            var stockItem = await _dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, ct);
            if (stockItem is not null)
                stockItem.CountOnHand += r.Quantity;
        }
    }
    if (reservations.Count > 0)
        await _dbContext.SaveChangesAsync(ct);
}
```

### BUG-004: Idempotent stock restore

```csharp
// Before (broken — restores stock every time, even if already released)
foreach (var r in expired)
{
    r.State = ReservationState.Expired;
    r.ModifiedAtUtc = now;
    if (r.StockLocationId is not null)
    {
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(...);
        if (stockItem is not null)
            stockItem.CountOnHand += r.Quantity; // DOUBLE-ADD on re-run
    }
}

// After (safe — only restores stock on first transition)
foreach (var r in expired)
{
    var wasActive = r.State == ReservationState.Reserved;
    r.State = ReservationState.Expired;
    r.ModifiedAtUtc = now;
    if (wasActive && r.StockLocationId is not null)
    {
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(...);
        if (stockItem is not null)
            stockItem.CountOnHand += r.Quantity;
    }
}
```

### BUG-003: Available stock vs raw on-hand

```csharp
// Before (broken — ignores reservations)
if (stockItem.CountOnHand < quantity)
    return StockItemResult.Errors.InsufficientStock;

// After
var activeReserved = await _dbContext.Set<StockReservation>()
    .Where(r => r.VariantId == variantId
        && r.StockLocationId == stockLocationId
        && r.State == ReservationState.Reserved
        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
    .SumAsync(r => r.Quantity, ct);
if (stockItem.CountOnHand - activeReserved < quantity)
    return StockItemResult.Errors.InsufficientStock;
```

### Edge Case: quantity zero in restock

`StockRestockService.RestockAsync(quantity: 0)` → returns `NegativeCountOnHand` (incorrect error code for zero). The guard `quantity <= 0` could use a dedicated `QuantityMustBePositive` error. Out of scope — preserved as-is.

### Edge Case: reservation with null StockLocationId

Multiple expiry/release paths check `if (r.StockLocationId is not null)` before restoring stock. Reservations at the "global" level (no specific location) are correctly excluded from stock restoration. This is by design.

## 10. Validation Criteria

- **VC-001**: `dotnet build` passes with warnings-as-errors (no new warnings).
- **VC-002**: All existing inventory unit tests pass: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Inventory"`.
- **VC-003**: New tests for each BUG/AC (≥ 1 test per bugfix requirement) pass.
- **VC-004**: No regression in Ordering module tests (since `DecrementStockAsync` behavior changes): `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`.
- **VC-005**: No regression in Catalog module tests: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Catalog"`.
- **VC-006**: EF Core migration is generated and applies cleanly against a fresh database.
- **VC-007**: The `ReservationExpiryService` BackgroundService is removed; `ReservationExpiryJob` Hangfire job remains as the sole expiry mechanism.
- **VC-008**: `dotnet test --filter "FullyQualifiedName~Concurrency"` passes for any new concurrency integration tests.

## 11. Related Specifications / Further Reading

- [spec-design-feature-conventions-remediation.md](spec-design-feature-conventions-remediation.md) — Feature Command/Query/Request/Response conventions
- [spec-design-admin-api-services.md](spec-design-admin-api-services.md) — Admin API service patterns
- `.harness/domains.yml` — Domain boundary definitions
- `docs/codebase/ARCHITECTURE.md` — Architecture overview and module isolation rules
- `docs/codebase/CONVENTIONS.md` — Coding conventions (Result objects, vertical slices)
