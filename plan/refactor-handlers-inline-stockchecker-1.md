---
goal: Inline StockChecker service logic into command/query handlers
version: 1.0
date_created: 2026-07-05
status: 'Planned'
tags: refactor, handlers, inventory, stockchecker, inline
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Move implementation logic from `StockChecker.cs` directly into the 7 command/query handlers that currently delegate to it via `IStockChecker`. `StockChecker.cs`, `IStockChecker.cs`, and the DI registration remain untouched. All existing `StockCheckerTests.cs` and `StockTransferServiceTests.cs` are preserved. New handler-level unit tests are created that test the inlined logic.

## 1. Requirements & Constraints

- **REQ-001**: `StockChecker.cs` and `IStockChecker.cs` must not be modified
- **REQ-002**: DI registration (`Inventory.Extension.cs` line 12 `services.AddScoped<IStockChecker, StockChecker>()`) must not be removed (still used by `ReservationExpiryService`)
- **REQ-003**: All 7 handlers must have `IStockChecker` dependency replaced by `IApplicationDbContext`
- **REQ-004**: All logic from each `StockChecker` method must be inlined directly in the handler's `Handle` method, not delegated
- **REQ-005**: Nested private method calls (`IsAvailableAsync` inside `ReserveForCartAsync`, `FulfillBackordersInternalAsync` inside `RestockAsync`) must be fully inlined with no local functions
- **REQ-006**: All domain factory/extension methods (`StockReservationExtensions.Reserve`, `StockMovementExtensions.Create`, `StockTransferExtensions.Transfer`, etc.) must be preserved as-is
- **REQ-007**: `SaveChangesAsync` must be called where the handler performs writes (commands); queries must NOT call `SaveChangesAsync`
- **REQ-008**: Logging in transfer handlers (`StockTransferLoggers.*`) must be preserved
- **REQ-009**: Response mapping logic must be preserved exactly
- **REQ-010**: Existing `StockCheckerTests.cs` and `StockTransferServiceTests.cs` must remain unchanged
- **REQ-011**: New handler test files must mirror the relevant test cases from `StockCheckerTests.cs` and `StockTransferServiceTests.cs`, adapted to test through the handler
- **PAT-001**: Follow the existing handler-level test pattern from `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Create/CreateStockItemTests.cs` — use `ApplicationDbContext` with InMemory database, instantiate handler directly, call `Handle`, verify results

## 2. Implementation Steps

### Implementation Phase 1: Storefront Cart Reservation Handlers

- GOAL-001: Refactor `GetCartReservations` (QueryHandler) and `ReserveCartStock` (CommandHandler) to inline StockChecker logic

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | **Refactor `GetCartReservations.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `GetReservationsForCartAsync` logic (lines 340-351 of StockChecker.cs): query `StockReservation` where `CartToken == request.CartToken && State == Reserved && ExpiresAtUtc > UtcNow`, project to `(Reservation, RemainingSeconds)` tuples, map to `List<Response>`. Remove `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-002 | **Refactor `ReserveCartStock.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `ReserveForCartAsync` + `IsAvailableAsync` logic (lines 37-61 and 240-264 of StockChecker.cs): (1) guard `quantity <= 0` → return `StockReservationResult.Errors.QuantityZero`; (2) query `StockItem` where `VariantId == variantId && StockLocationId == stockLocationId`; if null → `InsufficientStock`; (3) compute reserved = sum of `StockReservation.Quantity` where same variant/location, `State == Reserved`, `ExpiresAtUtc > UtcNow`; (4) check `CountOnHand - reserved >= quantity`; (5) call `StockReservationExtensions.Reserve(...)`, set `CartToken`, add to `_dbContext.Set<StockReservation>()`; (6) call `await dbContext.SaveChangesAsync(cancellationToken)`. Remove `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-003 | **Create handler tests for `GetCartReservations`** — Create `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Status/GetCartReservationsTests.cs`. Test cases: returns active reservations with remaining seconds, returns empty when no reservations, ignores expired reservations. Instantiate handler with `ApplicationDbContext` and `GetCartReservations.Query`. | | |
| TASK-004 | **Create handler tests for `ReserveCartStock`** — Create `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStockTests.cs`. Test cases: creates reservation with CartToken, uses custom TTL, returns failure when quantity zero, returns failure when insufficient stock, accounts for other active reservations. Instantiate handler with `ApplicationDbContext` and `ReserveCartStock.Command`. | | |

### Implementation Phase 2: Admin Transfer Handlers

- GOAL-002: Refactor `TransferStockTransfer`, `CancelStockTransfer`, and `ReceiveStockTransfer` (CommandHandlers) to inline StockChecker logic

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | **Refactor `TransferStockTransfer.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `ExecuteTransferAsync` logic (lines 470-523 of StockChecker.cs): (1) query `StockTransfer` with `.Include(t => t.TransferItems)` where `Id == command.Id`; (2) if null → `StockTransferResult.Failure.NotFound`; (3) validate source stock for each item; (4) call `transfer.Transfer()`; (5) decrement source stock items and create `StockMovement` records (transfer_out); (6) call `StockTransferLoggers.Transferred(logger, Id: command.Id)` on success; (7) call `await dbContext.SaveChangesAsync(cancellationToken)`. Remove `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-006 | **Refactor `CancelStockTransfer.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `CancelTransferAsync` logic (lines 579-610 of StockChecker.cs): (1) query `StockTransfer` with `.Include(t => t.TransferItems)` where `Id == command.Id`; (2) if null → `StockTransferResult.Failure.NotFound`; (3) check `wasInTransit = transfer.State == TransferState.InTransit`; (4) call `transfer.Cancel()`; (5) if `wasInTransit`, restore source stock items by adding `item.Quantity` back to `CountOnHand`; (6) call `StockTransferLoggers.Canceled(logger, Id: command.Id)` on success; (7) call `await dbContext.SaveChangesAsync(cancellationToken)`. Remove `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-007 | **Refactor `ReceiveStockTransfer.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `ReceiveTransferAsync` logic (lines 526-576 of StockChecker.cs): (1) query `StockTransfer` with `.Include(t => t.TransferItems)` where `Id == command.Id`; (2) if null → `StockTransferResult.Failure.NotFound`; (3) if state != InTransit → `StockTransferResult.Failure.InvalidStateTransition(...)`; (4) for each received item, call `transfer.Receive(variantId, quantity)`; (5) increment destination stock items and create `StockMovement` records (transfer_in); (6) call `StockTransferLoggers.Received(logger, Id: command.Id)` on success; (7) call `await dbContext.SaveChangesAsync(cancellationToken)`. Remove `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-008 | **Create handler tests for transfer handlers** — Create 3 test files: (a) `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransferTests.cs` (mirror transfer tests from `StockTransferServiceTests.cs` lines 72-139: decrement source + movement, not found, insufficient source, multiple items); (b) `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransferTests.cs` (mirror cancel tests lines 227-284: Draft cancel no restore, InTransit cancel restore, already Received failure); (c) `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransferTests.cs` (mirror receive tests lines 141-225: increment destination + movement, not found, not InTransit, exceeds transferred, partial receive). Use handler + `ApplicationDbContext` pattern. | | |

### Implementation Phase 3: Admin Stock Items Handlers

- GOAL-003: Refactor `GetStockSummary` (QueryHandler) and `RestockStockItem` (CommandHandler) to inline StockChecker logic

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | **Refactor `GetStockSummary.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `GetStockSummaryAsync` logic (lines 613-671 of StockChecker.cs): (1) query `StockItem` with `.Include(si => si.StockLocation)` where location is not deleted and active; (2) query `StockReservation` grouped by `(VariantId, StockLocationId)` for active reserved reservations; (3) build `VariantStockSummary` with location breakdown and totals; (4) map to `List<Response>`. Remove `using Module.Inventory.Services` and `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-010 | **Refactor `RestockStockItem.cs`** — Replace `IStockChecker stockChecker` with `IApplicationDbContext dbContext`. Inline `RestockAsync` + `FulfillBackordersInternalAsync` logic (lines 354-467 of StockChecker.cs): (1) guard `quantity <= 0` → `StockItemResult.Errors.NegativeCountOnHand`; (2) query `StockItem` by Id; if null → `StockItemResult.Errors.NotFound(...)`; (3) fulfill backorders: if `stockItem.Backorderable`, query `StockReservation` where same variant/location, `State == Reserved`, `ExpiresAtUtc > UtcNow`, ordered by `CreatedAtUtc`; iterate filling oldest first, updating state/quantity, creating backorder_fulfilled movements; (4) add remaining to `CountOnHand`; (5) create restock `StockMovement`; (6) build and return `RestockResult` → `Response`; (7) call `await dbContext.SaveChangesAsync(cancellationToken)`. Remove `using Module.Inventory.Services` and `using Module.Inventory.Services.Abstractions`. Add `using Shared.Operational.Persistence.Data`. | | |
| TASK-011 | **Create handler tests for `GetStockSummary`** — Create `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Summary/GetStockSummaryTests.cs`. Test cases (mirror from `StockCheckerTests.cs` lines 736-796): consolidated per-variant totals, flags low stock items, returns empty when no stock items. Instantiate handler with `ApplicationDbContext`. | | |
| TASK-012 | **Create handler tests for `RestockStockItem`** — Create `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Restock/RestockStockItemTests.cs`. Test cases (mirror from `StockCheckerTests.cs` lines 578-732): increase CountOnHand, failure when quantity zero, failure when stock item not found, fulfills backorders fully, partially fulfills backorders, does not fulfill when not backorderable, creates StockMovement with reference. Instantiate handler with `ApplicationDbContext`. | | |

## 3. Alternatives

- **ALT-001**: Keep `IStockChecker` delegation and decouple via adapter pattern — rejected because the goal is to eliminate the service call indirection, not to abstract it further
- **ALT-002**: Use MediatR pipeline behaviors to inject shared logic across handlers — rejected because the logic per handler is distinct enough that shared pipeline behavior would be over-engineered
- **ALT-003**: Extract inlined code into shared static helper methods — rejected per requirement to inline everything directly into handlers (no local functions)
- **ALT-004**: Delete `StockChecker.cs` entirely — rejected because the user explicitly requires StockChecker.cs to remain untouched

## 4. Dependencies

- **DEP-001**: `IApplicationDbContext` interface in `Shared.Operational.Persistence.Data` — must be accessible from Module/Inventory (already referenced)
- **DEP-002**: Domain entities and extension methods (`StockItem`, `StockReservation`, `StockTransfer`, `StockMovement`, `StockReservationExtensions`, `StockMovementExtensions`, `StockTransferExtensions`) — all in `Module.Inventory.Domain.*`
- **DEP-003**: Result types (`StockItemResult`, `StockReservationResult`, `StockTransferResult`, `RestockResult`, `BackorderFulfillmentResult`, `VariantStockSummary`, `LocationStockInfo`) — all in `Module.Inventory.Domain.*` or `Module.Inventory.Services.Models`
- **DEP-004**: `ReservationExpiryService` (background service) — still depends on `IStockChecker`, so DI registration must remain

## 5. Files

- **FILE-001**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.cs` — modify handler
- **FILE-002**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs` — modify handler
- **FILE-003**: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransfer.cs` — modify handler
- **FILE-004**: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransfer.cs` — modify handler
- **FILE-005**: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransfer.cs` — modify handler
- **FILE-006**: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.cs` — modify handler
- **FILE-007**: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs` — modify handler
- **FILE-008**: `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Status/GetCartReservationsTests.cs` — new test file
- **FILE-009**: `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStockTests.cs` — new test file
- **FILE-010**: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransferTests.cs` — new test file
- **FILE-011**: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransferTests.cs` — new test file
- **FILE-012**: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransferTests.cs` — new test file
- **FILE-013**: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Summary/GetStockSummaryTests.cs` — new test file
- **FILE-014**: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Restock/RestockStockItemTests.cs` — new test file
- **FILE-015** (untouched): `service/Api/src/Module/Inventory/Services/StockChecker.cs`
- **FILE-016** (untouched): `service/Api/src/Module/Inventory/Services/Abstractions/IStockChecker.cs`
- **FILE-017** (untouched): `service/Api/src/Module/Inventory/Inventory.Extension.cs`
- **FILE-018** (untouched): `service/Api/tests/Module.UnitTests/Inventory/Domain/Services/StockCheckerTests.cs`
- **FILE-019** (untouched): `service/Api/tests/Module.UnitTests/Inventory/Services/StockTransferServiceTests.cs`

## 6. Testing

- **TEST-001**: `StockCheckerTests.cs` — must continue to pass (StockChecker is unchanged)
- **TEST-002**: `StockTransferServiceTests.cs` — must continue to pass (StockChecker is unchanged)
- **TEST-003**: `ReservationExpiryServiceTests.cs` — must continue to pass (mocks IStockChecker, unchanged)
- **TEST-004**: `GetCartReservationsTests.cs` — new handler tests for cart reservation query
- **TEST-005**: `ReserveCartStockTests.cs` — new handler tests for cart reservation command
- **TEST-006**: `TransferStockTransferTests.cs` — new handler tests for transfer execution
- **TEST-007**: `CancelStockTransferTests.cs` — new handler tests for transfer cancellation
- **TEST-008**: `ReceiveStockTransferTests.cs` — new handler tests for transfer receiving
- **TEST-009**: `GetStockSummaryTests.cs` — new handler tests for stock summary query
- **TEST-010**: `RestockStockItemTests.cs` — new handler tests for restock command
- **TEST-011**: Build verification — `dotnet build` must succeed with no warnings (TreatWarningsAsErrors=true)
- **TEST-012**: Full unit test run — `dotnet test service/Api/tests/Module.UnitTests` must pass all tests

## 7. Risks & Assumptions

- **RISK-001**: Inlining duplicate code increases maintenance surface — if StockChecker logic changes, handlers must be updated separately. Mitigation: this is the accepted tradeoff per requirements.
- **RISK-002**: `SaveChangesAsync` is not called consistently in `StockChecker.cs` (only `ExpireReservationsAndRestoreStockAsync` calls it). The caller (`ReservationExpiryService`) calls it separately. Handlers that perform writes MUST call `SaveChangesAsync` within the `Handle` method to persist changes.
- **ASSUMPTION-001**: The `ApplicationDbContext` InMemory configuration (`UseInMemoryDatabase`, `AdditionalConfigurationsAssemblies`) used in existing tests is sufficient for handler-level tests.
- **ASSUMPTION-002**: All `using` directives for domain entities are already available through existing project references — no new NuGet packages needed.
- **ASSUMPTION-003**: The `BackorderFulfillmentResult` class is `internal` in `StockServiceModels.cs` — when inlined into the handler, the backorder fulfillment logic will be written directly without referencing this class, or it can be promoted to `public` (context: it was `internal` because only `StockChecker` used it).

## 8. Related Specifications / Further Reading

- `service/Api/src/Module/Inventory/Services/StockChecker.cs` — source of inlined logic (lines 37-61, 240-264, 340-351, 354-467, 470-523, 526-576, 579-610, 613-671)
- `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Create/CreateStockItemTests.cs` — reference test pattern
- `service/Api/tests/Module.UnitTests/Inventory/Domain/Services/StockCheckerTests.cs` — existing tests to mirror
- `service/Api/tests/Module.UnitTests/Inventory/Services/StockTransferServiceTests.cs` — existing transfer tests to mirror
