---
goal: Refactor StockLocation domain to split monolithic Method.cs by concerns, deduplicate activation/state logic, and expand test coverage
version: 1.0
date_created: 2026-07-05
last_updated: 2026-07-05
owner: Engineering
status: Completed
tags: refactor, domain, inventory, stocklocation, testing
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

StockLocation's domain methods (Create, Update, Activate, Deactivate, SoftDelete, Restore, SetAsDefault, StocksItem, FillStatus) are all in a single `StockLocation.Method.cs` file. This mirrors the problem solved for Product (see `refactor-product-extensions-methods-1.md`, `refactor-variant-extensions-methods-1.md`). The activation guard logic in `Update()` duplicates `Deactivate()`. The `ValidateStoreNotChanged` method lives in `StoreScoped.cs` (an entity-partial file). Validation tests only cover `Name` rules.

This plan splits `StockLocation.Method.cs` by concern into partial-class files matching the Catalog pattern, deduplicates activation logic, moves `ValidateStoreNotChanged` to a method concern file, and expands validation + method tests for full coverage.

## 1. Requirements & Constraints

- **REQ-001**: Split `StockLocation.Method.cs` into concern-specific partial-class files following `Product.Method.{Concern}.cs` convention
- **REQ-002**: Remove duplicate activation/deactivation guard logic between `Update()` and `Deactivate()` — `Update()` must delegate to `Deactivate()`/`Activate()` when the `active` parameter is provided
- **REQ-003**: Keep `StockLocation.StoreScoped.cs` for the `StoreId` property but move `ValidateStoreNotChanged` to a method concern file
- **REQ-004**: Expand `StockLocation.Validation.cs` tests to cover all 8 field rules (Name, Code, Address, City, Phone, PostalCode, AdminName, Presentation)
- **REQ-005**: Split `StockLocation.Extensions.Tests.cs` into concern-specific test files mirroring the new source structure
- **REQ-006**: All existing tests must pass after refactoring with no behavioral changes
- **REQ-007**: Add missing domain-level tests for edge cases: double-activation, double-deactivation, double-restore, double-softdelete, double-SetAsDefault, all FillStatus scenarios
- **CON-001**: `StockLocationMethod` class must be `partial` to allow cross-file extension
- **CON-002**: Existing namespace `Module.Inventory.Domain.StockLocations` must be preserved in all files
- **CON-003**: TreatWarningsAsErrors=true — no warnings allowed
- **PAT-001**: Follow `Product.Method.{Concern}.cs` pattern from Catalog module (see `service/Api/src/Module/Catalog/Domain/Products/Product.Method.Slugs.cs`, `Product.Method.Status.cs`, `Product.Method.Availability.cs`)
- **PAT-002**: Test file naming: `StockLocation.Method.{Concern}.Tests.cs` matching source
- **PAT-003**: All tests use `[Trait("Category", "Unit")]`, `[Trait("Module", "Inventory")]`, `[Trait("Entity", "StockLocation")]`
- **PAT-004**: Use `Shouldly` assertions and `FluentValidation.TestHelper` for validation tests
- **GUD-001**: Each new method file must declare `public static partial class StockLocationMethod` (not a new class)
- **GUD-002**: Deleted or renamed file references in the project must be updated — there are no `.csproj` glob issues since the SDK-style project auto-includes `*.cs`

## 2. Implementation Steps

### Implementation Phase 1: Split Source Files by Concern

- GOAL-001: Split the monolithic `StockLocation.Method.cs` into concern-specific partial files, deduplicate activation logic, and move `ValidateStoreNotChanged` out of `StoreScoped.cs`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Refactor `StockLocation.Method.cs` to keep only `Create` factory method; declare `public static partial class StockLocationMethod` | ✅ | 2026-07-05 |
| TASK-002 | Create `StockLocation.Method.Activation.cs` with `Activate()`, `Deactivate()`, and `Update()` (with delegated activation logic) | ✅ | 2026-07-05 |
| TASK-003 | Create `StockLocation.Method.Deletion.cs` with `SoftDelete()`, `Restore()` | ✅ | 2026-07-05 |
| TASK-004 | Create `StockLocation.Method.Default.cs` with `SetAsDefault()` | ✅ | 2026-07-05 |
| TASK-005 | Create `StockLocation.Method.Query.cs` with `StocksItem()`, `FillStatus()` | ✅ | 2026-07-05 |
| TASK-006 | Create `StockLocation.Method.Store.cs` with `ValidateStoreNotChanged()` — remove this method from `StockLocation.StoreScoped.cs` | ✅ | 2026-07-05 |
| TASK-007 | Keep `StockLocation.StoreScoped.cs` with only `StoreId` property | ✅ | 2026-07-05 |
| TASK-008 | Verify build passes: `dotnet build service/Api/src/Module/` | ✅ | 2026-07-05 |

### Implementation Phase 2: Deduplicate Activation Logic in Update

- GOAL-002: Eliminate the duplicated deactivation guard in `Update()` by delegating to `Deactivate()`/`Activate()`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | In `Update()`, replace the inline `active == false && location.Default` guard with a call to `location.Deactivate()` or `location.Activate()` based on the `active` parameter; store the result and propagate failure | ✅ | 2026-07-05 |
| TASK-010 | Ensure `Update()` does not set `location.Active` directly — delegate to `Activate()`/`Deactivate()` | ✅ | 2026-07-05 |
| TASK-011 | Verify build passes: `dotnet build service/Api/src/Module/` | ✅ | 2026-07-05 |

### Implementation Phase 3: Expand Validation Tests

- GOAL-003: Add FluentValidation test coverage for all 8 field rules

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Expand `StockLocation.Validation.Tests.cs`: add test classes for `CodeTooLong`, `AddressTooLong`, `CityTooLong`, `PhoneTooLong`, `PostalCodeTooLong`, `AdminNameTooLong`, `PresentationTooLong` following the `ApplyNameRules` pattern | ✅ | 2026-07-05 |
| TASK-013 | Run validation tests: `dotnet test --filter-class "*StockLocationValidation*"` — 33 tests pass | ✅ | 2026-07-05 |

### Implementation Phase 4: Split and Expand Method Tests

- GOAL-004: Split `StockLocation.Extensions.Tests.cs` into concern-specific test files and add edge-case coverage

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `StockLocation.Method.Factory.Tests.cs`: all Create variants (name required, with all params, with explicit id, default values) | ✅ | 2026-07-05 |
| TASK-015 | Create `StockLocation.Method.Activation.Tests.cs`: Activate (inactive→active, already active→ok), Deactivate (active→inactive, already inactive→ok, default→fail), Update activation delegation | ✅ | 2026-07-05 |
| TASK-016 | Create `StockLocation.Method.Deletion.Tests.cs`: SoftDelete (active→fail, inactive→ok, already deleted→ok), Restore (deleted→ok, not deleted→ok), verify timestamps | ✅ | 2026-07-05 |
| TASK-017 | Create `StockLocation.Method.Default.Tests.cs`: SetAsDefault (not default→ok, already default→ok) | ✅ | 2026-07-05 |
| TASK-018 | Create `StockLocation.Method.Query.Tests.cs`: StocksItem (found, not found, null stock items), FillStatus (full on_hand, partial with backorderable, partial without backorderable, no stock item, zero on_hand, negative on_hand) | ✅ | 2026-07-05 |
| TASK-019 | Create `StockLocation.Method.Store.Tests.cs`: ValidateStoreNotChanged (same store→ok, different store→fail, null original→ok, null new→ok) | ✅ | 2026-07-05 |
| TASK-020 | Delete old `StockLocation.Extensions.Tests.cs` | ✅ | 2026-07-05 |
| TASK-021 | Run all domain tests: `dotnet test --filter-class "Module.UnitTests.Inventory.Domain.StockLocations.*"` — 67 tests pass | ✅ | 2026-07-05 |

### Implementation Phase 5: Final Verification

- GOAL-005: Run full unit test suite and verify build

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Run `dotnet build service/Api/src/Module/` — 0 warnings, 0 errors | ✅ | 2026-07-05 |
| TASK-023 | Run `dotnet test service/Api/tests/Module.UnitTests` — all 2018 tests pass | ✅ | 2026-07-05 |
| TASK-024 | Run Inventory domain + feature tests — 67 domain + 12 feature tests pass | ✅ | 2026-07-05 |

## 3. Alternatives

- **ALT-001**: Keep all methods in a single `StockLocation.Method.cs` file. Rejected because it makes the file 237+ lines with unrelated concerns and diverges from the Catalog module pattern (`Product.Method.*.cs`).
- **ALT-002**: Keep `ValidateStoreNotChanged` in `StoreScoped.cs` alongside the `StoreId` property. Rejected because `StoreScoped.cs` is an entity-partial file (extends the class with a property), while methods belong in method-partial files.
- **ALT-003**: Rename all `Extensions.cs` files to `Method.cs` for consistency. Rejected as out of scope — this plan focuses only on StockLocation.
- **ALT-004**: Keep `StockLocation.StoreScoped.cs` with only the `StoreId` property and rename it to `StockLocation.Store.cs`. This is optional — the property could move into `StockLocation.cs` instead. Decision: keep separate file for clarity since multi-store scoping is a distinct concern.

## 4. Dependencies

- **DEP-001**: Existing `StockLocationMethod` class in `StockLocation.Method.cs` — must be made `partial`
- **DEP-002**: `StockLocationResult.Errors` and `StockLocationConstant` — no changes needed, just consumed
- **DEP-003**: No NuGet or project reference changes needed

## 5. Files

### Source Files (Modified)

| File | Action | Description |
|------|--------|-------------|
| `Domain/StockLocations/StockLocation.Method.cs` | MODIFY | Keep only `Create` factory; convert class to `partial` |
| `Domain/StockLocations/StockLocation.StoreScoped.cs` | MODIFY | Remove `ValidateStoreNotChanged`; keep only `StoreId` property |

### Source Files (Created)

| File | Description |
|------|-------------|
| `Domain/StockLocations/StockLocation.Method.Activation.cs` | `Activate()`, `Deactivate()`, `Update()` |
| `Domain/StockLocations/StockLocation.Method.Deletion.cs` | `SoftDelete()`, `Restore()` |
| `Domain/StockLocations/StockLocation.Method.Default.cs` | `SetAsDefault()` |
| `Domain/StockLocations/StockLocation.Method.Query.cs` | `StocksItem()`, `FillStatus()` |
| `Domain/StockLocations/StockLocation.Method.Store.cs` | `ValidateStoreNotChanged()` |

### Test Files (Created)

| File | Description |
|------|-------------|
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Method.Factory.Tests.cs` | Create tests |
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Method.Activation.Tests.cs` | Activate, Deactivate, Update delegation tests |
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Method.Deletion.Tests.cs` | SoftDelete, Restore tests |
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Method.Default.Tests.cs` | SetAsDefault tests |
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Method.Query.Tests.cs` | StocksItem, FillStatus tests |
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Method.Store.Tests.cs` | ValidateStoreNotChanged tests |

### Test Files (Deleted)

| File | Description |
|------|-------------|
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Extensions.Tests.cs` | Replaced by per-concern test files |

### Test Files (Expanded)

| File | Description |
|------|-------------|
| `tests/Module.UnitTests/Inventory/Domain/StockLocations/StockLocation.Validation.Tests.cs` | Add 7 missing field rule test classes |

## 6. Testing

- **TEST-001**: `StockLocation.Method.Factory.Tests.cs` — Verify Create returns correct properties, defaults, accepts explicit id, all parameter combinations
- **TEST-002**: `StockLocation.Method.Activation.Tests.cs` — Activate (inactive→active, already active→idempotent ok), Deactivate (active→inactive, already inactive→idempotent ok, default→error), Update delegates to Deactivate/Activate for active parameter
- **TEST-003**: `StockLocation.Method.Deletion.Tests.cs` — SoftDelete (active→error, inactive→deleted, already deleted→idempotent ok, verify timestamps), Restore (deleted→restored, not deleted→idempotent ok, verify timestamps cleared)
- **TEST-004**: `StockLocation.Method.Default.Tests.cs` — SetAsDefault (not default→becomes default, already default→idempotent ok)
- **TEST-005**: `StockLocation.Method.Query.Tests.cs` — StocksItem (found, not found, null StockItems), FillStatus (full on_hand, partial + backorderable, partial + not backorderable, no stock item, zero on_hand, negative on_hand)
- **TEST-006**: `StockLocation.Method.Store.Tests.cs` — ValidateStoreNotChanged (same store→ok, different store→error, null original store→ok, null new store→ok)
- **TEST-007**: `StockLocation.Validation.Tests.cs` — Expand from 1 field rule (Name) to all 8 field rules; test max-length boundary for each field; test valid values pass

## 7. Risks & Assumptions

- **RISK-001**: Delegating `Update()` activation to `Deactivate()`/`Activate()` may change return behavior because `Deactivate()` returns `Result.Ok(successMessage)` while `Update()` returns `Result.Ok()`. Mitigation: `Update()` retains its own return type and only uses the guard/state-mutation from `Deactivate()`/`Activate()`, not their return value.
- **RISK-002**: `StockLocation.StoreScoped.cs` is referenced by EF Core configuration (`StockLocationConfiguration.cs`) for the `StoreId` column mapping — keeping the property in a partial file on the same class is fine since EF Core navigates the full `StockLocation` type.
- **ASSUMPTION-001**: No other files reference `StockLocationMethod` or `StockLocationMethod` by class name — all usages are via `StockLocationMethod.Create(...)` or `location.Update(...)` which remain unchanged.
- **ASSUMPTION-002**: The SDK-style project (`*.csproj`) auto-includes all `*.cs` files in the directory tree, so no project file edits are needed for new or removed files.

## 8. Related Specifications / Further Reading

- `plan/refactor-product-extensions-methods-1.md` — precedent for splitting `ProductMethod` by concern
- `plan/refactor-variant-extensions-methods-1.md` — precedent for splitting `VariantMethod` by concern
- `service/Api/src/Module/Catalog/Domain/Products/Product.Method.Status.cs` — example concern file
- `service/Api/src/Module/Catalog/Domain/Products/Product.Method.Availability.cs` — example concern file
- `service/Api/src/Module/Catalog/Domain/Products/Product.Method.Slugs.cs` — example concern file
