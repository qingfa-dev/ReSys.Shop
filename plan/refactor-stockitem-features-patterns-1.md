---
goal: Refactor StockItem Features to Follow Location Module Patterns
version: 1.0
date_created: 2026-07-06
owner: AI Agent
status: 'Planned'
tags: refactor, inventory, stock-items, patterns, alignment
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Refactor the StockItem feature layer (shared models, mappings, validators) and action folders (Create, Update, Delete, GetById, GetPaged, BulkAdjust, etc.) to match the Location module conventions already applied to StockLocations. The plan converts class-based models to records, adds audit fields, flattens nested folders, splits validators per-field, aligns handler return types, and expands test coverage.

## 1. Requirements & Constraints

- **REQ-001**: All Shared model types (`StockItemParameters`, `StockItemRequest`, `StockItemDetailResponse`, `StockItemListItemResponse`) must use `record` types with `{ get; init; }` properties.
- **REQ-002**: `StockItemDetailResponse` and `StockItemListItemResponse` must expose `CreatedBy` and `ModifiedBy` fields.
- **REQ-003**: Domain mapping (`StockItem.Mapping.Domain.cs`) must call `AuditableBehavior.Create()` / `AuditableBehavior.Touch()`.
- **REQ-004**: Model mapping (`StockItem.Mapping.Model.cs`) must project `CreatedBy` and `ModifiedBy`.
- **REQ-005**: Validator must be split from one monolithic file into per-field files (`CountOnHand`, `Backorderable`) matching the `StockLocation.Validator.{Field}.cs` naming.
- **REQ-006**: Validation rule definitions must move from domain `StockItemValidation` to feature-layer per-field files as `internal static Apply{Field}Rules`.
- **REQ-007**: Action-specific Request/Response classes that inherit from the shared types must be converted from `class` to `record`.
- **REQ-008**: `Get/ById/` must flatten to `GetById/`; `Get/Paged/` must flatten to `GetPaged/`.
- **REQ-009**: `DeleteStockItem` must return `ICommand<Response>` with a mapped `DeleteStockItem.Response` record (matching `DeleteCountry` / `DeleteStockLocation` pattern).
- **REQ-010**: Missing validator files must be created: `GetPagedStockItems.Validator.cs`, `RestockStockItem.Validator.cs`, `ImportStockItems.Validator.cs`, `LowStock/GetLowStockItems.Validator.cs`, `Summary/GetStockSummary.Validator.cs`.
- **REQ-011**: Missing response files must be created: `DeleteStockItem.Response.cs`, `BulkAdjust/BulkAdjustStockItems.Response.cs`.
- **REQ-012**: `TreatWarningsAsErrors = true` — zero warnings allowed.
- **CON-001**: `BulkAdjustStockItems`, `LowStock`, `Summary`, `Import`, and `Restock` features exist outside the standard CRUD pattern — they may retain their specific command/query signatures as long as shared models align.
- **CON-002**: The domain-layer `StockItemValidation` class may be kept for AdminName/Presentation-style validations if referenced elsewhere, or trimmed if only consumed by the feature layer.
- **GUD-001**: Follow `StockLocation.Validator.{Field}.cs` naming for per-field validator files.
- **GUD-002**: Follow `StockLocation` test patterns for new test files.
- **PAT-001**: Action folder structure: flat directory per action (e.g., `GetById/` not `Get/ById/`).
- **PAT-002**: Handler returns `ICommand<Response>` / `ICommandHandler<Command, Response>` with mapped response from entity.

## 2. Implementation Steps

### Implementation Phase 1 — Shared Models to Records with Audit Fields

- GOAL-001: Convert all Shared model types to records, add `CreatedBy`/`ModifiedBy`, and use `{ get; init; }` properties.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Convert `StockItemParameters` from `abstract class` to `abstract record class` in `StockItem.Model.Parameters.cs` | | |
| TASK-002 | Convert `StockItemRequest` from `class : StockItemParameters` to `record : StockItemParameters` in `StockItem.Model.Request.cs` | | |
| TASK-003 | Convert `StockItemDetailResponse` from `class : StockItemParameters` to `record : StockItemParameters` in `StockItem.Model.Response.cs`; change `{ get; set; }` to `{ get; init; }`; add `string? CreatedBy { get; init; }` and `string? ModifiedBy { get; init; }`; add `using Shared.Application.Domain.Concerns.Auditable;` | | |
| TASK-004 | Convert `StockItemListItemResponse` from `class : StockItemParameters` to `record : StockItemParameters` in `StockItem.Model.Response.cs`; change `{ get; set; }` to `{ get; init; }`; add `string? CreatedBy { get; init; }` and `string? ModifiedBy { get; init; }` | | |
| TASK-005 | Build Module to verify no errors after response model changes | | |

### Implementation Phase 2 — Shared Mappings with AuditableBehavior and Audit Fields

- GOAL-002: Update domain mapping to use `AuditableBehavior.Create()`/`Touch()` and model mapping to project audit fields.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Add `using Shared.Application.Domain.Concerns.Auditable;` to `StockItem.Mapping.Domain.cs`; after `StockItemMethod.Create(...)` success, call `AuditableBehavior.Create(result.Value)`; after `stockItem.Update(...)` success, call `AuditableBehavior.Touch(stockItem)` | | |
| TASK-007 | Add `CreatedBy = entity.CreatedBy` and `ModifiedBy = entity.ModifiedBy` to `MapToDetail<T>()` in `StockItem.Mapping.Model.cs` | | |
| TASK-008 | Add `CreatedBy = entity.CreatedBy` and `ModifiedBy = entity.ModifiedBy` to `MapToListItem<T>()` in `StockItem.Mapping.Model.cs` | | |
| TASK-009 | Build Module to verify mapping changes compile | | |

### Implementation Phase 3 — Validator Split Per-Field

- GOAL-003: Split monolithic `StockItem.Validator.cs` into per-field files; move rule definitions from domain `StockItemValidation` to feature-layer per-field files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Create `StockItem.Validator.Parameters.cs` — copy `StockItemParametersValidator` + `ApplyStockItemParametersRules` from old `StockItem.Validator.cs`; remove the domain-layer `using Module.Inventory.Domain.StockLocations.StockItems;` since per-field files handle those imports | | |
| TASK-011 | Create `StockItem.Validator.CountOnHand.cs` — `internal static ApplyCountOnHandRules` with `GreaterThanOrEqualTo(0)` using `StockItemResult.Errors.NegativeCountOnHand` | | |
| TASK-012 | Create `StockItem.Validator.Backorderable.cs` — `internal static ApplyBackorderableRules` with `NotNull()` | | |
| TASK-013 | Delete the old monolithic `StockItem.Validator.cs` | | |
| TASK-014 | Remove `ApplyCountOnHandRules` and `ApplyBackorderableRules` from domain-layer `StockItemValidation.cs` (keep only if domain methods are referenced elsewhere — check via grep) | | |
| TASK-015 | Build Module to verify validator split compiles | | |

### Implementation Phase 4 — Action-Specific Records and Missing Files

- GOAL-004: Convert all action-specific Request/Response classes to records; create missing Response files; update Delete handler to return `ICommand<Response>`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Convert `CreateStockItem.Request` from `class : StockItemRequest` to `record : StockItemRequest` in `Create/CreateStockItem.Request.cs` | | |
| TASK-017 | Convert `CreateStockItem.Response` from `class : StockItemDetailResponse` to `record : StockItemDetailResponse` in `Create/CreateStockItem.Response.cs` | | |
| TASK-018 | Convert `UpdateStockItem.Request` from `class : StockItemRequest` to `record : StockItemRequest` in `Update/UpdateStockItem.Request.cs` | | |
| TASK-019 | Convert `UpdateStockItem.Response` from `class : StockItemDetailResponse` to `record : StockItemDetailResponse` in `Update/UpdateStockItem.Response.cs` | | |
| TASK-020 | Convert `Get/ById/GetStockItemById.Response` from `sealed class : StockItemDetailResponse` to `record : StockItemDetailResponse` in `GetStockItemById.Response.cs` | | |
| TASK-021 | Convert `Get/Paged/GetPagedStockItems.Response` from `class : StockItemListItemResponse` to `record : StockItemListItemResponse` in `GetPagedStockItems.Response.cs` | | |
| TASK-022 | Convert `LowStock/GetLowStockItems.Response` from `sealed class : StockItemListItemResponse` to `record : StockItemListItemResponse` in `GetLowStockItems.Response.cs`; change `{ get; set; }` to `{ get; init; }` | | |
| TASK-023 | Convert `BulkAdjust/BulkAdjustStockItems.Request` from `class : StockItemRequest` to `record : StockItemRequest` in `BulkAdjustStockItems.Request.cs` | | |
| TASK-024 | Create `Delete/DeleteStockItem.Response.cs` — `public record Response : StockItemListItemResponse;` | | |
| TASK-025 | Create `BulkAdjust/BulkAdjustStockItems.Response.cs` — `public record Response;` (empty record for ICommand<Response> compliance) | | |
| TASK-026 | Update `Delete/DeleteStockItem.cs`: change `Command` from `ICommand` to `ICommand<Response>`; change handler from `ICommandHandler<Command>` to `ICommandHandler<Command, Response>`; add `using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;`; change `return Result.Ok();` to `return entity.MapToListItem<Response>();` | | |
| TASK-027 | Update `Delete/DeleteStockItem.Endpoint.cs`: update `Produces<Result>()` to `Produces<Result<Response>>()` | | |
| TASK-028 | Build Module to verify all record conversions and new files compile | | |

### Implementation Phase 5 — Folder Flattening and Missing Validators

- GOAL-005: Flatten `Get/ById/` → `GetById/` and `Get/Paged/` → `GetPaged/`; create missing validator files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Create `GetById/` directory; write all 4 files (`GetStockItemById.cs`, `.Endpoint.cs`, `.Response.cs`, `.Validator.cs`) with namespace `...StockItems.GetById` (no nested `Get.ById`) | | |
| TASK-030 | Create `GetPaged/` directory; write all 5 files (`GetPagedStockItems.cs`, `.Endpoint.cs`, `.Parameters.cs`, `.Response.cs`, `.Validator.cs`) with namespace `...StockItems.GetPaged` (no nested `Get.Paged`) | | |
| TASK-031 | Delete old `Get/` directory with all children | | |
| TASK-032 | Create `GetPaged/GetPagedStockItems.Validator.cs` — validate `Parameters` is not null | | |
| TASK-033 | Create `Restock/RestockStockItem.Validator.cs` — validate `Request` is not null, `Quantity > 0` | | |
| TASK-034 | Create `Import/ImportStockItems.Validator.cs` — validate `File` is not null | | |
| TASK-035 | Create `LowStock/GetLowStockItems.Validator.cs` — validate `Threshold >= 0` if provided | | |
| TASK-036 | Create `Summary/GetStockSummary.Validator.cs` — no parameters to validate (empty validator) | | |
| TASK-037 | Build Module to verify folder flattening and new validators compile | | |

### Implementation Phase 6 — Test Updates and Missing Tests

- GOAL-006: Update existing tests for new namespaces/types; add missing handler and validator tests.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Update existing `CreateStockItem.Tests.cs` namespace to remain compatible (it already uses `Create` folder) — verify no changes needed | | |
| TASK-039 | Move existing `Get/Paged/GetPagedStockItems.Tests.cs` to `GetPaged/GetPagedStockItems.Tests.cs` with updated namespace `...StockItems.GetPaged` | | |
| TASK-040 | Delete old test folder `Get/` | | |
| TASK-041 | Create `GetById/GetStockItemById.Tests.cs` — handler test with found + not found cases | | |
| TASK-042 | Create `Delete/DeleteStockItem.Tests.cs` — handler test with delete success, not found, and (optional) business validation | | |
| TASK-043 | Create `Update/UpdateStockItem.Tests.cs` — handler test with update success + not found | | |
| TASK-044 | Create `GetPaged/GetPagedStockItems.Validator.Tests.cs` — validate null params, valid params | | |
| TASK-045 | Create `BulkAdjust/BulkAdjustStockItems.Tests.cs` — handler test (basic success) | | |
| TASK-046 | Build + run full Module unit tests | | |

### Implementation Phase 7 — Final Verification

- GOAL-007: Full solution build and comprehensive test suite execution.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-047 | Build full solution (`dotnet build`) — 0 warnings, 0 errors | | |
| TASK-048 | Run `Module.UnitTests` — all pass | | |
| TASK-049 | Run `Shared.UnitTests` — all pass | | |

## 3. Alternatives

- **ALT-001**: Keep nested `Get/ById/` and `Get/Paged/` folders — rejected because Location module uses flat folders (`GetById/`, `GetPagedOrAll/`), and StockLocations was already flattened.
- **ALT-002**: Leave Delete handler returning `ICommand` (void) — rejected because Location module's Delete returns `ICommand<Response>` with a mapped list-item response, providing better API consistency.
- **ALT-003**: Keep monolithic validator — rejected because Location module and StockLocations now split validators per-field for maintainability and discoverability.
- **ALT-004**: Keep domain `StockItemValidation` methods and create parallel feature-layer copies — rejected; matching the Location module pattern means the feature-layer should own validation rules directly.

## 4. Dependencies

- **DEP-001**: None — this refactor is self-contained within the Inventory module's StockItem feature layer and tests.
- **DEP-002**: The `AuditableBehavior` class must exist in `Shared.Application.Domain.Concerns.Auditable` (confirmed present from StockLocation mapping).
- **DEP-003**: StockItem domain entity, factory, and result types must remain stable (no changes needed).

## 5. Files

### Source Files Modified
- **FILE-001**: `StockItem.Model.Parameters.cs` — class → record
- **FILE-002**: `StockItem.Model.Request.cs` — class → record
- **FILE-003**: `StockItem.Model.Response.cs` — class → record, +CreatedBy/ModifiedBy
- **FILE-004**: `StockItem.Mapping.Domain.cs` — +AuditableBehavior calls
- **FILE-005**: `StockItem.Mapping.Model.cs` — +CreatedBy/ModifiedBy mapping
- **FILE-006**: `StockItem.Validator.cs` — deleted (split)
- **FILE-007**: `StockItemValidation.cs` (domain) — trimmed if unused
- **FILE-008**: `DeleteStockItem.cs` — ICommand → ICommand<Response>
- **FILE-009**: `CreateStockItem.Request.cs` — class → record
- **FILE-010**: `CreateStockItem.Response.cs` — class → record
- **FILE-011**: `UpdateStockItem.Request.cs` — class → record
- **FILE-012**: `UpdateStockItem.Response.cs` — class → record
- **FILE-013**: `BulkAdjustStockItems.Request.cs` — class → record
- **FILE-014**: `GetLowStockItems.Response.cs` — class → record
- **FILE-015**: Various Get/ById/* files — deleted (replaced by GetById/*)
- **FILE-016**: Various Get/Paged/* files — deleted (replaced by GetPaged/*)
- **FILE-017**: DeleteStockItem.Endpoint.cs — Produces<Result> → Produces<Result<Response>>

### Source Files Created
- **FILE-018**: `StockItem.Validator.Parameters.cs` — composer
- **FILE-019**: `StockItem.Validator.CountOnHand.cs` — per-field
- **FILE-020**: `StockItem.Validator.Backorderable.cs` — per-field
- **FILE-021**: `DeleteStockItem.Response.cs`
- **FILE-022**: `BulkAdjustStockItems.Response.cs`
- **FILE-023**: `GetPagedStockItems.Validator.cs`
- **FILE-024**: `RestockStockItem.Validator.cs`
- **FILE-025**: `ImportStockItems.Validator.cs`
- **FILE-026**: `GetLowStockItems.Validator.cs`
- **FILE-027**: `GetStockSummary.Validator.cs`
- **FILE-028**: `GetStockItemById.cs` (GetById/)
- **FILE-029**: `GetStockItemById.Endpoint.cs` (GetById/)
- **FILE-030**: `GetStockItemById.Response.cs` (GetById/)
- **FILE-031**: `GetStockItemById.Validator.cs` (GetById/)
- **FILE-032**: `GetPagedStockItems.cs` (GetPaged/)
- **FILE-033**: `GetPagedStockItems.Endpoint.cs` (GetPaged/)
- **FILE-034**: `GetPagedStockItems.Parameters.cs` (GetPaged/)
- **FILE-035**: `GetPagedStockItems.Response.cs` (GetPaged/)
- **FILE-036**: `GetPagedStockItems.Validator.cs` (GetPaged/)

### Test Files Modified
- **FILE-037**: `Get/Paged/GetPagedStockItems.Tests.cs` → moved to `GetPaged/GetPagedStockItems.Tests.cs`

### Test Files Created
- **FILE-038**: `GetById/GetStockItemById.Tests.cs`
- **FILE-039**: `Delete/DeleteStockItem.Tests.cs`
- **FILE-040**: `Update/UpdateStockItem.Tests.cs`
- **FILE-041**: `GetPaged/GetPagedStockItems.Validator.Tests.cs`
- **FILE-042**: `BulkAdjust/BulkAdjustStockItems.Tests.cs`

### Files Deleted
- **FILE-043**: `StockItem.Validator.cs` (monolithic)
- **FILE-044**: `Get/ById/*` (all 4 files)
- **FILE-045**: `Get/Paged/*` (all 4 files + test)
- **FILE-046**: Old `Get/` test directory

## 6. Testing

- **TEST-001**: `CreateStockItem.Tests.cs` — verify handler still works with record types (no test changes expected)
- **TEST-002**: `UpdateStockItem.Tests.cs` — NEW: handler test for update success + not found
- **TEST-003**: `DeleteStockItem.Tests.cs` — NEW: handler test for delete success + not found + validation
- **TEST-004**: `GetStockItemById.Tests.cs` — NEW: handler test for found + not found
- **TEST-005**: `GetPagedStockItems.Tests.cs` — MOVED: same content, updated namespace
- **TEST-006**: `GetPagedStockItems.Validator.Tests.cs` — NEW: validator test
- **TEST-007**: `BulkAdjustStockItems.Tests.cs` — NEW: handler test
- **TEST-008**: `StockItem.Mapping.Tests.cs` — verify CreatedBy/ModifiedBy are mapped (update existing)
- **TEST-009**: Domain `StockItemValidation` tests — verify per-field validator behavior if domain tests exist

## 7. Risks & Assumptions

- **RISK-001**: Changing shared model types from `class` to `record` may affect serialization behavior — `record` types in C# are serializable and should be compatible with System.Text.Json.
- **RISK-002**: The `BulkAdjustStockItems` handler currently uses `ICommand` — changing to `ICommand<Response>` requires the endpoint to produce `Result<Response>` instead of `Result`. This affects API contract but matches the Location module pattern.
- **RISK-003**: `RestockStockItem.Response` currently inherits from domain service type `RestockResult`. Converting to `record` may conflict with the base class if it's a class. If `RestockResult` is a class, `Response` cannot be a record that inherits it (C# constraint). In that case, keep `Response` as a class or refactor `RestockResult`.
- **ASSUMPTION-001**: All existing feature test classes follow the `IDisposable` + `ApplicationDbContext` + `InMemoryDatabase` pattern with `AdditionalConfigurationsAssemblies` — new test files follow the same pattern.
- **ASSUMPTION-002**: `RestockResult` and `VariantStockSummary` (base classes for Restock and Summary responses) are defined in `Module.Inventory.Services` namespace and may not be convertible to record — these features may retain class-based responses if necessary.

## 8. Related Specifications / Further Reading

- `/plan/refactor-stocklocation-features-patterns-1.md` — Completed plan for StockLocation feature refactoring (same patterns applied here)
- `service/Api/src/Module/Location/Features/Admin/Countries/` — Reference implementation for the target patterns
- `service/Api/src/Module/Inventory/Features/Admin/StockLocations/` — Reference for already-refactored Inventory feature
