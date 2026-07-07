---
goal: Remove BuildingBlocksCompat.cs by migrating all call sites to canonical Shared API
version: 2.0
date_created: 2026-07-07
status: 'Completed'
tags: refactor, cleanup, querying, backward-compatibility
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Two backward-compatibility adapter files (`BuildingBlocksCompat.cs` and `FluentValidationCompat.cs`) still exist in the Shared project, providing shim APIs for code that hasn't been migrated to the canonical Shared querying pattern. This plan removes both files by refactoring all 16 handler files (Promotions, Shipping, Payment, Ordering) and 4 validator files (Shipping) to use the direct `ParseAll()` → `ApplyQuerying()` → `ToPagedOrAllAsync(model, projection)` pattern, and inlines 6 FluentValidation rules.

Supersedes `plan/refactor-remove-buildingblocks-compat-3.md`.

## 1. Requirements & Constraints

- **REQ-001**: Delete `Shared/Application/Mediators/BuildingBlocksCompat.cs` (2 adapter classes: `BuildingBlocksResultCompat`, `BuildingBlocksQueryCompat`; 6 methods total)
- **REQ-002**: Delete `Shared/Application/Validations/FluentValidationCompat.cs` (1 adapter class: `FluentValidationCompat`; 2 methods)
- **REQ-003**: Remove `global using Shared.Application.Mediators;` from `Module/GlobalUsing.cs:16`
- **REQ-004**: Remove `global using Shared.Application.Validations;` from `Module/GlobalUsing.cs:21`
- **REQ-005**: Refactor all 16 handler files using the compat pattern to the canonical three-step pattern
- **REQ-006**: Inline `ApplyPageValidation()` and `ApplyPageSizeValidation()` calls in 4 Shipping validator files
- **CON-001**: All other modules (Catalog, Location, Identity, Inventory, Profile) already use canonical `ParseAll()` pattern — no changes needed
- **CON-002**: Each handler's `Query` record property named `Parameters` (type `QueryingParameters`) stays unchanged; only the query pipeline logic changes
- **CON-003**: The `BuildingBlocksResultCompat.Failures()` extension is dead code — the `Result`/`Result<T>` types already have `.Failures` as a built-in property (`Result.Method.cs:189`, `ValueResult.cs:62`)
- **CON-004**: The `PageIndex` extension on `QueryingParameters` has zero callers — no migration needed

## 2. Implementation Steps

### Implementation Phase 1: Migrate Promotions module handlers (6 files)

- GOAL-001: Replace the `ApplyQueryOptions(parameters)` + `ToPagedOrAllAsync(projection, parameters)` compat pattern with the canonical `ParseAll()` → `ApplyQuerying(model)` → `ToPagedOrAllAsync(model, projection)` pattern in all 6 Promotions paged query handlers.

**Canonical pattern to apply in every file:**

BEFORE:
```csharp
var parameters = request.Parameters;
var pagedResult = await dbContext.Set<T>()
    .AsNoTracking()
    .Where(...)
    .OrderBy(...)
    .ApplyQueryOptions(parameters)
    .Select(...)
    .ToPagedOrAllAsync(x => x, parameters, cancellationToken);
return pagedResult;
```

AFTER:
```csharp
var parsing = request.Parameters.ParseAll();
if (parsing.IsFailure)
    return new PagedResult<Response>([], new PageModel());
var pagedResult = await dbContext.Set<T>()
    .AsNoTracking()
    .Where(...)
    .OrderBy(...)
    .ApplyQuerying(parsing.Value)
    .Select(...)
    .ToPagedOrAllAsync(parsing.Value, x => x, cancellationToken);
return pagedResult;
```

Key changes:
1. Add `var parsing = request.Parameters.ParseAll();` + early return on failure
2. Replace `.ApplyQueryOptions(parameters)` → `.ApplyQuerying(parsing.Value)`
3. Replace `.ToPagedOrAllAsync(x => x, parameters, ct)` → `.ToPagedOrAllAsync(parsing.Value, x => x, ct)` (model parameter BEFORE projection)
4. For handlers using `ToPagedOrAllAsync(projection, parameters, ct)` with a real projection (not `x => x`), replace with `.ToPagedOrAllAsync(parsing.Value, projection, ct)`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Refactor `Module/Promotions/Features/Storefront/Promotions/ListActivePromotions.cs:26-32` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x, parameters, ct)` → canonical pattern | | |
| TASK-002 | Refactor `Module/Promotions/Features/Admin/Promotions/Get/Paged/GetPagedPromotions.cs:25-26` — same transformation | | |
| TASK-003 | Refactor `Module/Promotions/Features/Admin/PromotionRules/Get/All/GetPromotionRules.cs:21-22` — same transformation | | |
| TASK-004 | Refactor `Module/Promotions/Features/Admin/PromotionCategories/Get/Paged/GetPagedPromotionCategories.cs:23-24` — same transformation | | |
| TASK-005 | Refactor `Module/Promotions/Features/Admin/PromotionActions/Get/All/GetPromotionActions.cs:21-22` — same transformation | | |
| TASK-006 | Refactor `Module/Promotions/Features/Admin/CouponCodes/Get/Paged/GetPagedCouponCodes.cs:23-24` — same transformation | | |

### Implementation Phase 2: Migrate Shipping module handlers (4 files)

- GOAL-002: Same canonical pattern transformation for Shipping module paged query handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Refactor `Module/Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.cs:25,38` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x, parameters, ct)` | | |
| TASK-008 | Refactor `Module/Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethods.cs:26-27` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parameters, ct)` | | |
| TASK-009 | Refactor `Module/Shipping/Features/Admin/Shipments/Get/Paged/GetPagedShipments.cs:25-26` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(s => s.MapToListItem<Response>(), parameters, ct)` | | |
| TASK-010 | Refactor `Module/Shipping/Features/Admin/MethodRates/Get/Paged/GetMethodRates.cs:23,32` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x, parameters, ct)` | | |

### Implementation Phase 3: Migrate Payment module handlers (3 files)

- GOAL-003: Same canonical pattern transformation for Payment module paged query handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Refactor `Module/Payment/Features/Storefront/Payment/Methods/ListPaymentMethods.cs:23,25` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x, parameters, ct)` | | |
| TASK-012 | Refactor `Module/Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.cs:24,27` — `.ApplyQueryOptions(request.Parameters)` + `.ToPagedOrAllAsync(projection, request.Parameters, ct)` where projection is `x => new Response { ... }` | | |
| TASK-013 | Refactor `Module/Payment/Features/Admin/PaymentMethods/Get/Paged/GetPagedPaymentMethods.cs:27-28` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parameters, ct)` | | |

### Implementation Phase 4: Migrate Ordering module handlers (3 files)

- GOAL-004: Same canonical pattern transformation for Ordering module paged query handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Refactor `Module/Ordering/Features/Admin/Orders/Get/Paged/GetPagedOrders.cs:28-29` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x.MapToListItem<Response>(), parameters, ct)`. Note: uses custom `Parameters : QueryingParameters` record. | | |
| TASK-015 | Refactor `Module/Ordering/Features/Admin/Orders/Get/Adjustments/GetOrderAdjustments.cs:20,24` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x, parameters, ct)` | | |
| TASK-016 | Refactor `Module/Ordering/Features/Admin/Orders/Shipments/Get/GetOrderShipments.cs:21,31` — `.ApplyQueryOptions(parameters)` + `.ToPagedOrAllAsync(x => x, parameters, ct)` | | |

### Implementation Phase 5: Migrate validators (4 files)

- GOAL-005: Replace `ApplyPageValidation()` and `ApplyPageSizeValidation()` calls with inline `.GreaterThanOrEqualTo(1)`.

**Pattern:**
BEFORE:
```csharp
RuleFor(x => x.Parameters.PageNumber).ApplyPageValidation();
RuleFor(x => x.Parameters.PageSize).ApplyPageSizeValidation();
```
AFTER:
```csharp
RuleFor(x => x.Parameters.PageNumber).GreaterThanOrEqualTo(1);
RuleFor(x => x.Parameters.PageSize).GreaterThanOrEqualTo(1);
```

No `using` import needed — `GreaterThanOrEqualTo` comes from `FluentValidation` which is already in `GlobalUsing.cs`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Refactor `Module/Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.Validator.cs:9-10` | | |
| TASK-018 | Refactor `Module/Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethods.Validator.cs:9-10` | | |
| TASK-019 | Refactor `Module/Shipping/Features/Admin/Shipments/Get/Paged/GetPagedShipments.Validator.cs:9-10` | | |
| TASK-020 | Refactor `Module/Shipping/Features/Admin/MethodRates/Get/Paged/GetMethodRates.Validator.cs:11-12` | | |

### Implementation Phase 6: Remove adapter files and global usings

- GOAL-006: Delete the two compat files and remove their now-unnecessary global using directives.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Delete `Shared/Application/Mediators/BuildingBlocksCompat.cs` | | |
| TASK-022 | Delete `Shared/Application/Validations/FluentValidationCompat.cs` | | |
| TASK-023 | Remove `global using Shared.Application.Mediators;` from `Module/GlobalUsing.cs:16` | | |
| TASK-024 | Remove `global using Shared.Application.Validations;` from `Module/GlobalUsing.cs:21` | | |

### Implementation Phase 7: Verify build

- GOAL-007: Confirm all projects build with zero errors and warnings (warnings-as-errors enforced).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Run `dotnet build service/Api/src/Api/Api.csproj` — 0 errors | | |
| TASK-026 | Run `dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — 0 errors | | |
| TASK-027 | Run `dotnet build service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj` — 0 errors | | |

## 3. Alternatives

- **ALT-001**: Keep the compat layer and incrementally migrate. Rejected because the compat layer adds "magic" behavior (unexpected extension method resolution via `Shared.Application.Mediators` global using) and makes the codebase harder to understand for new developers who expect the canonical `ParseAll()` → `ApplyQuerying()` → `ToPagedOrAllAsync(model)` pattern.
- **ALT-002**: Keep `FluentValidationCompat.cs` since it's only 2 methods. Rejected because inlining `.GreaterThanOrEqualTo(1)` is 1 line per validator and eliminates the need for the `Shared.Application.Validations` namespace import entirely.

## 4. Dependencies

- **DEP-001**: `PagedResult<T>` must support `new PagedResult<Response>([], new PageModel())` constructor. Verify at `Shared/Application/Models/Results/PagedResult.cs`. If not available, use `default(PagedResult<T>)` as fallback.
- **DEP-002**: `QueryingModel` + `ApplyQuerying()` accessible via `Shared.Operational.Persistence.Specifications.Querying` (already in `Module/GlobalUsing.cs:25`).
- **DEP-003**: `PageModel` accessible via `Shared.Operational.Persistence.Specifications.Paging` (check if already in global usings; if not, add or use fully qualified name).
- **DEP-004**: `.Failures` built-in property exists on `Result`/`Result<T>` — confirmed at `Result.Method.cs:189` and `ValueResult.cs:62`.

## 5. Files

- **FILE-001** (DELETE): `service/Api/src/Shared/Application/Mediators/BuildingBlocksCompat.cs` — 52 lines, defines `BuildingBlocksResultCompat` + `BuildingBlocksQueryCompat`
- **FILE-002** (DELETE): `service/Api/src/Shared/Application/Validations/FluentValidationCompat.cs` — 12 lines, defines `FluentValidationCompat`
- **FILE-003** (MODIFY): `service/Api/src/Module/GlobalUsing.cs` — remove lines 16 and 21 (`Shared.Application.Mediators` and `Shared.Application.Validations`)
- **FILE-004–009** (MODIFY): 6 Promotions handler files (see TASK-001–006)
- **FILE-010–013** (MODIFY): 4 Shipping handler files (see TASK-007–010)
- **FILE-014–016** (MODIFY): 3 Payment handler files (see TASK-011–013)
- **FILE-017–019** (MODIFY): 3 Ordering handler files (see TASK-014–016)
- **FILE-020–023** (MODIFY): 4 Shipping validator files (see TASK-017–020)

## 6. Testing

- **TEST-001**: `dotnet build service/Api/src/Api/Api.csproj` — 0 compilation errors, 0 warnings
- **TEST-002**: `dotnet build service/Api/tests/Module.UnitTests` — 0 compilation errors, 0 warnings
- **TEST-003**: `dotnet build service/Api/tests/Shared.UnitTests` — 0 compilation errors, 0 warnings
- **TEST-004**: `grep -r "BuildingBlocksCompat\|BuildingBlocksResultCompat\|BuildingBlocksQueryCompat\|FluentValidationCompat" service/Api/src/` — returns 0 matches after cleanup
- **TEST-005**: Verify no `ApplyQueryOptions`, `ApplyPageValidation`, or `ApplyPageSizeValidation` calls remain
- **TEST-006**: Run `dotnet test service/Api/tests/Module.UnitTests` — all tests pass

## 7. Risks & Assumptions

- **RISK-001**: `ToPagedOrAllAsync(model, projection, ct)` parameter order is `(model, projection, ct)` — model BEFORE projection. This differs from the compat order `(projection, parameters, ct)`. Incorrect ordering will cause compilation errors due to type mismatch.
- **RISK-002**: Some handler files may have additional custom logic between `.ApplyQueryOptions(parameters)` and `.ToPagedOrAllAsync(projection, parameters)` that requires careful handling. Verify each handler individually.
- **RISK-003**: The `PagedResult<T>` constructor `new PagedResult<Response>([], new PageModel())` must exist for the early-return-on-failure pattern. If not, the correct TypeScript-like expression in C# is `PagedResult<Response>.Empty` or use `default`.
- **ASSUMPTION-001**: All 16 handler files follow the same two-step compat pattern: `.ApplyQueryOptions(parameters)` followed by `.ToPagedOrAllAsync(projection, parameters)`, with no intervening query operations.
- **ASSUMPTION-002**: Handlers using `.Select(projection).ToPagedOrAllAsync(x => x, parameters)` can simplify by passing the real projection directly: `.ToPagedOrAllAsync(model, projection)`.
- **ASSUMPTION-003**: The `Shared.Application.Mediators` global using is only needed for the compat classes — removing it won't break any other code.

## 8. Related Specifications / Further Reading

- `plan/refactor-remove-buildingblocks-compat-3.md` — superseded by this plan
- `plan/refactor-buildingblocks-migration-1.md` — previous migration that added the compat layer
- `Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.Extensions.cs` — `ParseAll()` documentation
- `Shared/Operational/Persistence/Specifications/Querying/Querying.Model.ApplyExtensions.cs` — `ApplyQuerying()` and `ToPagedOrAllAsync()` documentation
- `Shared/Application/Models/Results/Result.Method.cs` — `.Failures` built-in property (line 189)
- `Shared/Application/Models/Results/ValueResult.cs` — `.Failures` built-in property (line 62)
- `Shared/Operational/Persistence/Specifications/Paging/Extensions/Page.Model.EfCore.Extensions.cs` — `ToPagedOrAllAsync(projection, PageModel)` implementation
