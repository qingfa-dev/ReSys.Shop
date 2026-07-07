---
goal: Refactor StockItem domain to split monolithic Extensions.cs by concerns, rename to Method.cs convention, deduplicate query methods, and expand test coverage
version: 1.0
date_created: 2026-07-05
last_updated: 2026-07-05
owner: Engineering
status: Completed
tags: refactor, domain, inventory, stockitem, testing
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

StockItem's domain methods (Create, Update, AdjustCountOnHand, SetBackorderable, Restock, Pick, IsAvailable, CanSupply, TotalOnHand, IsBackorderable, ProcessBackorders, FillStatus) are all in a single `StockItem.Extensions.cs` (233 lines) with inconsistent file naming — the class is `StockItemMethod` but the file is `Extensions.cs`. This diverges from the StockLocation convention (`.Method.cs`) and StockMovement convention (`.Method.cs`). Three methods (`IsAvailable`/`CanSupply`, `TotalOnHand`/`CountOnHand`, `IsBackorderable`/`Backorderable`) have identical or trivial purpose.

This plan renames the file to `StockItem.Method.cs`, splits by concern into partial-class files matching the established pattern, documents duplicate-purpose methods as deprecation candidates, and expands test coverage for untested methods.

## 1. Requirements & Constraints

- **REQ-001**: Rename `StockItem.Extensions.cs` to `StockItem.Method.cs` and convert class to `partial`
- **REQ-002**: Split `StockItem` methods into concern-specific partial-class files following `StockLocation.Method.{Concern}.cs` convention
- **REQ-003**: Document (do not delete) duplicate-purpose methods `IsAvailable`/`CanSupply`, `TotalOnHand`, `IsBackorderable` as deprecation candidates in code comments
- **REQ-004**: Expand `StockItem.Validation.Tests.cs` to cover `ApplyBackorderableRules`
- **REQ-005**: Split `StockItem.Method.Tests.cs` into concern-specific test files and add edge-case coverage for untested methods
- **REQ-006**: All existing tests must pass after refactoring with no behavioral changes
- **CON-001**: `StockItemMethod` class must be `partial` to allow cross-file extension
- **CON-002**: Existing namespace `Module.Inventory.Domain.StockLocations.StockItems` must be preserved in all files
- **CON-003**: TreatWarningsAsErrors=true — no warnings allowed
- **PAT-001**: Follow `StockLocation.Method.{Concern}.cs` pattern from this repo (see `plan/refactor-stocklocation-methods-concerns-1.md`)
- **PAT-002**: File naming must be `StockItem.Method.{Concern}.cs` (not `Extensions.cs`)
- **PAT-003**: Test file naming: `StockItem.Method.{Concern}.Tests.cs` matching source
- **PAT-004**: All tests use `[Trait("Category", "Unit")]`, `[Trait("Module", "Inventory")]`, `[Trait("Entity", "StockItem")]`
- **PAT-005**: Use `Shouldly` assertions and `FluentValidation.TestHelper` for validation tests
- **GUD-001**: Each new method file must declare `public static partial class StockItemMethod` (not a new class)
- **GUD-002**: SDK-style project auto-includes all `*.cs` files — no `.csproj` edits needed

## 2. Implementation Steps

### Implementation Phase 1: Rename and Split Source Files by Concern

- GOAL-001: Rename `StockItem.Extensions.cs` to `StockItem.Method.cs` with only `Create` factory, then create concern-specific partial files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename `StockItem.Extensions.cs` → `StockItem.Method.cs`; keep only `Create` factory method; convert class to `public static partial class StockItemMethod` | ✅ | 2026-07-05 |
| TASK-002 | Create `StockItem.Method.Adjustment.cs` with `Update()`, `AdjustCountOnHand()`, `SetBackorderable()`, `Restock()`, `Pick()` | ✅ | 2026-07-05 |
| TASK-003 | Create `StockItem.Method.Query.cs` with `IsAvailable()`, `CanSupply()`, `TotalOnHand()`, `IsBackorderable()`, `ProcessBackorders()`, `FillStatus()`; add deprecation comments on duplicates | ✅ | 2026-07-05 |
| TASK-004 | Verify build passes: `dotnet build service/Api/src/Module/` | ✅ | 2026-07-05 |

### Implementation Phase 2: Expand Validation Tests

- GOAL-002: Add FluentValidation test coverage for `ApplyBackorderableRules`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Expand `StockItem.Validation.Tests.cs`: add `StockItemValidationBackorderableTests` class testing `ApplyBackorderableRules` (true, false) | ✅ | 2026-07-05 |
| TASK-006 | Run validation tests: `dotnet test --filter-class "*StockItemValidation*"` — 5 tests pass | ✅ | 2026-07-05 |

### Implementation Phase 3: Split and Expand Method Tests

- GOAL-003: Split `StockItem.Method.Tests.cs` into concern-specific test files and add edge-case coverage for untested methods

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create `StockItem.Method.Factory.Tests.cs`: Create (valid countOnHand, negative countOnHand, default values, unique ids) | ✅ | 2026-07-05 |
| TASK-008 | Create `StockItem.Method.Adjustment.Tests.cs`: Update (valid, negative), AdjustCountOnHand (positive, negative valid, negative invalid), SetBackorderable (true, false), Restock (positive, zero, negative), Pick (sufficient, exact, insufficient, zero, negative) | ✅ | 2026-07-05 |
| TASK-009 | Create `StockItem.Method.Query.Tests.cs`: IsAvailable (3 scenarios), CanSupply (3 scenarios), TotalOnHand, IsBackorderable (true, false), FillStatus (3 scenarios), ProcessBackorders (4 scenarios) | ✅ | 2026-07-05 |
| TASK-010 | Delete old `StockItem.Method.Tests.cs` | ✅ | 2026-07-05 |
| TASK-011 | Run all domain tests: `dotnet test --filter-class "Module.UnitTests.Inventory.Domain.StockItems.*"` — 40 tests pass | ✅ | 2026-07-05 |

### Implementation Phase 4: Final Verification

- GOAL-004: Run full unit test suite and verify build

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Run `dotnet build service/Api/src/Module/` — 0 warnings, 0 errors | ✅ | 2026-07-05 |
| TASK-013 | Run `dotnet test service/Api/tests/Module.UnitTests` — all 2038 tests pass | ✅ | 2026-07-05 |

## 3. Alternatives

- **ALT-001**: Delete duplicate-purpose methods (`IsAvailable`/`CanSupply`, `TotalOnHand`, `IsBackorderable`). Rejected because they are used by feature handlers and the Ruby SDK alignment convention requires their presence. Marked as deprecation candidates instead.
- **ALT-002**: Keep `StockItem.Extensions.cs` filename and only split internally. Rejected because it diverges from `StockLocation.Method.cs`, `StockMovement.Method.cs`, and `Product.Method.cs` conventions.
- **ALT-003**: Leave all methods in a single file. Rejected because 233-line monolithic file with 12 unrelated methods violates the concern-splitting pattern established by StockLocation and Product.

## 4. Dependencies

- **DEP-001**: Existing `StockItemMethod` class in `StockItem.Extensions.cs` — must be made `partial` and file renamed
- **DEP-002**: `StockItemResult.Errors` and `StockItemConstant` — no changes needed, just consumed
- **DEP-003**: No NuGet or project reference changes needed

## 5. Files

### Source Files (Modified)

| File | Action | Description |
|------|--------|-------------|
| `Domain/StockLocations/StockItems/StockItem.Extensions.cs` | RENAME→MODIFY | Rename to `StockItem.Method.cs`; keep only `Create` factory; convert to `partial` |

### Source Files (Created)

| File | Description |
|------|-------------|
| `Domain/StockLocations/StockItems/StockItem.Method.Adjustment.cs` | `Update()`, `AdjustCountOnHand()`, `SetBackorderable()`, `Restock()`, `Pick()` |
| `Domain/StockLocations/StockItems/StockItem.Method.Query.cs` | `IsAvailable()`, `CanSupply()`, `TotalOnHand()`, `IsBackorderable()`, `ProcessBackorders()`, `FillStatus()` |

### Test Files (Created)

| File | Description |
|------|-------------|
| `tests/Module.UnitTests/Inventory/Domain/StockItems/StockItem.Method.Factory.Tests.cs` | Create tests |
| `tests/Module.UnitTests/Inventory/Domain/StockItems/StockItem.Method.Adjustment.Tests.cs` | Update, AdjustCountOnHand, SetBackorderable, Restock, Pick tests |
| `tests/Module.UnitTests/Inventory/Domain/StockItems/StockItem.Method.Query.Tests.cs` | IsAvailable, CanSupply, TotalOnHand, IsBackorderable, FillStatus, ProcessBackorders tests |

### Test Files (Deleted)

| File | Description |
|------|-------------|
| `tests/Module.UnitTests/Inventory/Domain/StockItems/StockItem.Method.Tests.cs` | Replaced by per-concern test files |

### Test Files (Expanded)

| File | Description |
|------|-------------|
| `tests/Module.UnitTests/Inventory/Domain/StockItems/StockItem.Validation.Tests.cs` | Add `ApplyBackorderableRules` test class |

## 6. Testing

- **TEST-001**: `StockItem.Method.Factory.Tests.cs` — Create with valid countOnHand, negative countOnHand→error, defaults, unique ids
- **TEST-002**: `StockItem.Method.Adjustment.Tests.cs` — Update (valid, negative→error), AdjustCountOnHand (positive, negative valid, negative invalid→error, idempotent), SetBackorderable (true, false), Restock (positive, zero→error, negative→error), Pick (sufficient, exact, insufficient→error, zero→error, negative→error)
- **TEST-003**: `StockItem.Method.Query.Tests.cs` — IsAvailable (3 scenarios), CanSupply (3 scenarios — same as IsAvailable to verify parity), TotalOnHand (returns CountOnHand), IsBackorderable (true, false), FillStatus (full on_hand, partial + backorderable, partial + not backorderable), ProcessBackorders (sufficient stock, insufficient + backorderable, insufficient + not backorderable, zero quantity)
- **TEST-004**: `StockItem.Validation.Tests.cs` — Add `ApplyBackorderableRules` (true passes, false passes)

## 7. Risks & Assumptions

- **RISK-001**: Renaming `StockItem.Extensions.cs` to `StockItem.Method.cs` may break feature handler imports if any file uses `using static StockItemMethod` or references the filename. Mitigation: the class name `StockItemMethod` does not change, only the filename. SDK-style projects use file content, not filenames, for compilation.
- **RISK-002**: `StockItem.Method.Tests.cs` tests class `StockItemMethodTests` — after deleting and recreating, the new test classes have different names, but trait filtering (`Entity=StockItem`) remains the same.
- **ASSUMPTION-001**: No feature handler references `StockItemMethod` by namespace path — all usages are via `StockItemMethod.Create(...)` or `item.Update(...)` which remain unchanged.
- **ASSUMPTION-002**: The duplicate methods `IsAvailable`/`CanSupply` may eventually be consolidated. For now, a code comment marks them as deprecation candidates without changing behavior.

## 8. Related Specifications / Further Reading

- `plan/refactor-stocklocation-methods-concerns-1.md` — precedent for splitting `StockLocationMethod` by concern
- `service/Api/src/Module/Inventory/Domain/StockLocations/StockItems/StockItem.Extensions.cs` — current monolithic source
- `service/Api/src/Module/Inventory/Domain/StockLocations/StockLocation.Method.Activation.cs` — example concern file for reference
- `service/Api/src/Module/Catalog/Domain/Products/Product.Method.Status.cs` — Catalog module concern file for reference
