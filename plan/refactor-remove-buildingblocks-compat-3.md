---
goal: Remove BuildingBlocksCompat.cs by migrating all call sites to direct Shared API usage
version: 1.0
date_created: 2026-07-07
status: 'Planned'
tags: refactor, cleanup, querying
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Two backward-compatibility adapter files (`BuildingBlocksCompat.cs` and `FluentValidationCompat.cs`) still exist in the Shared project, providing shim APIs for code that hasn't been migrated to the canonical Shared querying pattern. This plan removes both files by refactoring all 16 handler files (Promotions, Shipping, Payment, Ordering) and 4 validator files (Shipping) to use the direct `ParseAll()` → `ApplyQuerying()` → `ToPagedOrAllAsync(model)` pattern, and inlines 6 FluentValidation rules.

## 1. Requirements & Constraints

- **REQ-001**: Delete `Shared/Application/Mediators/BuildingBlocksCompat.cs` (2 adapter classes, 4 methods)
- **REQ-002**: Delete `Shared/Application/Validations/FluentValidationCompat.cs` (1 adapter class, 2 methods)
- **REQ-003**: Remove `global using Shared.Application.Mediators;` from `Module/GlobalUsing.cs` (added for compat)
- **REQ-004**: Remove `global using Shared.Application.Validations;` from `Module/GlobalUsing.cs` (added for compat)
- **REQ-005**: Refactor all 16 handler files currently using `ApplyQueryOptions(parameters)` + `ToPagedOrAllAsync(projection, parameters)` to use `parameters.ParseAll()` → `.ApplyQuerying(model)` → `.ToPagedOrAllAsync(model, projection)`
- **REQ-006**: The `.Failures` property is already built into `Result`/`Result<T>` — no migration needed (the compat extension was redundant). Only verify no code depends on the extension method specifically.
- **REQ-007**: `PageIndex` extension method is dead code (0 callers) — no migration needed.
- **REQ-008**: Replace `ApplyPageValidation()` and `ApplyPageSizeValidation()` calls with inline `.GreaterThanOrEqualTo(1)` in 4 validator files
- **CON-001**: Only the 16 files listed in Section 5 need `ApplyQueryOptions`/`ToPagedOrAllAsync` pattern changes — all other modules (Catalog, Location, Identity, Inventory, Profile) already use the canonical `ParseAll()` pattern.
- **CON-002**: Each handler's `Query` record property named `Parameters` (type `QueryingParameters`) stays unchanged; only the query pipeline logic changes.

## 2. Implementation Steps

### Implementation Phase 1: Migrate Promotions module handlers (6 files)

- GOAL-001: Replace the `ApplyQueryOptions` + `ToPagedOrAllAsync(QueryingParameters)` pattern with the canonical `ParseAll()` → `ApplyQuerying(model)` → `ToPagedOrAllAsync(model, projection)` pattern in all 6 Promotions paged query handlers.

Each handler follows this exact transformation:

**BEFORE:**
```csharp
public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
{
    var query = dbContext.Set<T>()
        .AsNoTracking()
        .OrderBy(...)
        .ApplyQueryOptions(request.Parameters);

    var pagedResult = await query
        .ToPagedOrAllAsync(x => Project<T, Response>(), request.Parameters, cancellationToken);

    return pagedResult;
}
```

**AFTER:**
```csharp
public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
{
    var parsing = request.Parameters.ParseAll();
    if (parsing.IsFailure)
        return new PagedResult<Response>([], new PageModel());

    var pagedResult = await dbContext.Set<T>()
        .AsNoTracking()
        .OrderBy(...)
        .ApplyQuerying(parsing.Value)
        .ToPagedOrAllAsync(parsing.Value, x => Project<T, Response>(), cancellationToken);

    return pagedResult;
}
```

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Refactor `Promotions/Features/Storefront/Promotions/ListActivePromotions.cs` — replace `ApplyQueryOptions(parameters)` + `ToPagedOrAllAsync(x => x, parameters, ct)` with `ParseAll()` + `ApplyQuerying(model)` + `ToPagedOrAllAsync(model, x => x, ct)` | | |
| TASK-002 | Refactor `Promotions/Features/Admin/Promotions/Get/Paged/GetPagedPromotions.cs` — same pattern | | |
| TASK-003 | Refactor `Promotions/Features/Admin/PromotionRules/Get/All/GetPromotionRules.cs` — same pattern | | |
| TASK-004 | Refactor `Promotions/Features/Admin/PromotionCategories/Get/Paged/GetPagedPromotionCategories.cs` — same pattern | | |
| TASK-005 | Refactor `Promotions/Features/Admin/PromotionActions/Get/All/GetPromotionActions.cs` — same pattern | | |
| TASK-006 | Refactor `Promotions/Features/Admin/CouponCodes/Get/Paged/GetPagedCouponCodes.cs` — same pattern | | |

### Implementation Phase 2: Migrate Shipping module handlers (4 files)

- GOAL-002: Same transformation for Shipping module paged query handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Refactor `Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.cs` | | |
| TASK-008 | Refactor `Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethods.cs` | | |
| TASK-009 | Refactor `Shipping/Features/Admin/Shipments/Get/Paged/GetPagedShipments.cs` | | |
| TASK-010 | Refactor `Shipping/Features/Admin/MethodRates/Get/Paged/GetMethodRates.cs` | | |

### Implementation Phase 3: Migrate Payment module handlers (3 files)

- GOAL-003: Same transformation for Payment module paged query handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Refactor `Payment/Features/Storefront/Payment/Methods/ListPaymentMethods.cs` | | |
| TASK-012 | Refactor `Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.cs` | | |
| TASK-013 | Refactor `Payment/Features/Admin/PaymentMethods/Get/Paged/GetPagedPaymentMethods.cs` | | |

### Implementation Phase 4: Migrate Ordering module handlers (3 files)

- GOAL-004: Same transformation for Ordering module paged query handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Refactor `Ordering/Features/Admin/Orders/Get/Paged/GetPagedOrders.cs` | | |
| TASK-015 | Refactor `Ordering/Features/Admin/Orders/Get/Adjustments/GetOrderAdjustments.cs` | | |
| TASK-016 | Refactor `Ordering/Features/Admin/Orders/Shipments/Get/GetOrderShipments.cs` | | |

### Implementation Phase 5: Migrate validators (4 files)

- GOAL-005: Replace `ApplyPageValidation()` and `ApplyPageSizeValidation()` calls with inline `.GreaterThanOrEqualTo(1)`.

**BEFORE:**
```csharp
RuleFor(x => x.Parameters.PageNumber).ApplyPageValidation();
RuleFor(x => x.Parameters.PageSize).ApplyPageSizeValidation();
```

**AFTER:**
```csharp
RuleFor(x => x.Parameters.PageNumber).GreaterThanOrEqualTo(1);
RuleFor(x => x.Parameters.PageSize).GreaterThanOrEqualTo(1);
```

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Refactor `Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.Validator.cs` | | |
| TASK-018 | Refactor `Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethods.Validator.cs` | | |
| TASK-019 | Refactor `Shipping/Features/Admin/Shipments/Get/Paged/GetPagedShipments.Validator.cs` | | |
| TASK-020 | Refactor `Shipping/Features/Admin/MethodRates/Get/Paged/GetMethodRates.Validator.cs` | | |

### Implementation Phase 6: Remove adapter files and global usings

- GOAL-006: Delete the two compat files and remove their global using directives.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Delete `Shared/Application/Mediators/BuildingBlocksCompat.cs` | | |
| TASK-022 | Delete `Shared/Application/Validations/FluentValidationCompat.cs` | | |
| TASK-023 | Remove `global using Shared.Application.Mediators;` from `Module/GlobalUsing.cs` | | |
| TASK-024 | Remove `global using Shared.Application.Validations;` from `Module/GlobalUsing.cs` | | |

### Implementation Phase 7: Verify build

- GOAL-007: Confirm all projects build with zero errors.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Run `dotnet build service/Api/src/Api/Api.csproj` — 0 errors | | |
| TASK-026 | Run `dotnet build tests/Module.UnitTests` — 0 errors | | |
| TASK-027 | Run `dotnet build tests/Shared.UnitTests` — 0 errors | | |

## 3. Alternatives

- **ALT-001**: Keep the compat layer and incrementally migrate. Rejected because the compat layer adds "magic" behavior (unexpected extension method resolution) and makes the codebase harder to understand for new developers who expect the canonical `ParseAll()` pattern.
- **ALT-002**: Keep `FluentValidationCompat.cs` since it's only 2 methods. Rejected because inlining `.GreaterThanOrEqualTo(1)` is 1 line per validator and eliminates the need for the namespace import.

## 4. Dependencies

- **DEP-001**: The `PagedResult<T>` constructor must accept `(IEnumerable<T> Items, PageModel page)` — verify this is the case.
- **DEP-002**: The `QueryingModel` record must be accessible via `Shared.Operational.Persistence.Specifications.Querying` (already in Module global usings).
- **DEP-003**: `.Failures` must be a built-in property on `Result`/`Result<T>` (already added in refactor-buildingblocks-migration-1, confirmed via `Result.Method.cs:188` and `ValueResult.cs:62`).

## 5. Files

- **FILE-001** (DELETE): `Shared/Application/Mediators/BuildingBlocksCompat.cs`
- **FILE-002** (DELETE): `Shared/Application/Validations/FluentValidationCompat.cs`
- **FILE-003** (MODIFY): `Module/GlobalUsing.cs` — remove 2 global usings
- **FILE-004** through **FILE-009** (MODIFY): 6 Promotions handler files
- **FILE-010** through **FILE-013** (MODIFY): 4 Shipping handler files
- **FILE-014** through **FILE-016** (MODIFY): 3 Payment handler files
- **FILE-017** through **FILE-019** (MODIFY): 3 Ordering handler files
- **FILE-020** through **FILE-023** (MODIFY): 4 Shipping validator files

## 6. Testing

- **TEST-001**: `dotnet build src/Api` — 0 compilation errors (no regression)
- **TEST-002**: `dotnet build tests/Module.UnitTests` — 0 compilation errors
- **TEST-003**: `dotnet build tests/Shared.UnitTests` — 0 compilation errors
- **TEST-004**: Verify no remaining references to `BuildingBlocksCompat` or `FluentValidationCompat` anywhere in the codebase

## 7. Risks & Assumptions

- **RISK-001**: The `PagedResult<T>` constructor for `new PagedResult<T>([], new PageModel())` must exist. If it doesn't, use `default(PagedResult<T>)` instead.
- **RISK-002**: Some handler files may have additional custom logic between the query construction and pagination steps that differs from the standard pattern. Each file must be read and understood before applying the transformation.
- **ASSUMPTION-001**: All 16 handler files follow the same pattern: query setup → `.ApplyQueryOptions(parameters)` → `.ToPagedOrAllAsync(projection, parameters)`, with no intervening operations.
- **ASSUMPTION-002**: The `.Failures` built-in property on `Result`/`Result<T>` is functionally identical to the now-removed compat extension methods.

## 8. Related Specifications / Further Reading

- `plan/refactor-buildingblocks-migration-1.md` — previous migration that added the compat layer
- `Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.Extensions.cs` — `ParseAll()` documentation
- `Shared/Operational/Persistence/Specifications/Querying/Querying.Model.ApplyExtensions.cs` — `ApplyQuerying()` and `ToPagedOrAllAsync()` documentation
- `Shared/Application/Models/Results/Result.Method.cs` — `.Failures` built-in property (line 188)
- `Shared/Application/Models/Results/ValueResult.cs` — `.Failures` built-in property (line 62)
