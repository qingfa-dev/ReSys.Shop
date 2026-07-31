---
goal: Add domain query whitelist constants and wire full querying (search/sort/filter) into the EF-native candidate list endpoints, following the established {Entity}Constant.Query + ParseAll + ApplyQuerying pattern.
version: 1.0
date_created: 2026-07-31
last_updated: 2026-07-31
owner: ReSys.Shop Engineering
status: 'Planned'
tags: [`refactor`, `querying`, `whitelist`, `catalog`, `inventory`]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This plan follows the review "which endpoint could be applied full querying parameters" performed against the 15 list endpoints converted in the pagedresult-envelope-standardization effort. It adds the missing domain `Query` whitelist constant and upgrades the three **EF-native** candidate handlers — `GetAllStockItems`, `ListVariantsByProduct`, `ListVariantImages` — from paging-only to full querying (filter/search/sort) using the established infrastructure (`QueryingParameters.ParseAll` + `IQueryable.ApplyQuerying`). The in-memory paging handlers are explicitly excluded with documented technical rationale.

## 1. Requirements & Constraints

- **REQ-001**: Every candidate entity whose `{Entity}.Constant.cs` lacks a `Query` class must gain one declaring `AllowedSearchFields`, `AllowedSortFields`, and `AllowedFilterFields`.
- **REQ-002**: Each EF-native candidate handler must accept `filter`, `search`, `searchFields`, `searchMode`, and `sort` query-string parameters via the existing `Parameters : QueryingParameters` record, parsed with `ParseAll` and applied with `ApplyQuerying`.
- **REQ-003**: Existing default ordering must be preserved for stable paging when the caller supplies no `sort` (via `ApplyQuerying` `defaultSortClauses`).
- **REQ-004**: The HTTP surface must not change: endpoints keep `PagedResult<T>` envelopes and `Parameters : QueryingParameters`; no `Endpoint.cs` file is modified by this plan.
- **SEC-001**: Field whitelists are the enforcement mechanism — a `filter`/`search`/`sort` referencing a disallowed field must produce a validation failure result (never a silently-ignored or arbitrary-projection query).
- **CON-001**: `ApplyFilter`/`ApplySearch`/`ApplySort`/`ApplyQuerying` are `IQueryable<T>`-only extensions; **no `IEnumerable<T>` (in-memory) overloads exist** in `Shared.Operational.Persistence.Specifications.*`. Handlers that page in-memory cannot use them without new infrastructure.
- **CON-002**: `TreatWarningsAsErrors=true` globally — any warning fails the build.
- **CON-003**: `Shared.Operational.Persistence.Specifications.Paging`, `.Paging.Extensions`, and `.Querying` are global usings (Module/GlobalUsing.cs:25-27). `Shared.Operational.Persistence.Specifications.Sorting` is **not** global — handlers using `SortClause` must add that `using` explicitly.
- **CON-004**: Feature directories are `Storefront` (never `Store`); all 3 targets already comply.
- **GUD-001**: Canonical reference implementations: `GetPagedOrders.cs:22-34`, `GetPagedStockReservations.cs:24-35`, `ListShippingRates.cs` (Storefront `ParseAll` + projection precedent).
- **GUD-002**: Preserve existing `// Contract:`, `// Load:`, `// Map:`, `// Filter:` comment lines verbatim.
- **GUD-003**: Match each entity's existing `Constant.cs` declaration style — `readonly string[]` for VariantImage (matches sibling Catalog Variant/TaxonRule constants), `HashSet`-based `IReadOnlySet<string>` where that style already exists.
- **PAT-001**: Standard handler pattern — `ParseAll(allowedFilterFields, allowedSearchFields, allowedSortFields)` → `IsFailure` guard returning `parsing.Errors` → `ApplyQuerying(model, defaultSortClauses)` → `ToPagedOrAllAsync(model, projection, ct)`.
- **PAT-002**: Stable default sort expressed as `[new SortClause { Field = nameof(Entity.Field) }]` passed to `ApplyQuerying`; the pre-existing explicit `.OrderBy(...)` is removed so EF never sees two `OrderBy` calls.

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Declare the missing `VariantImageConstant.Query` whitelist so `ListVariantImages` can be upgraded.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add a `Query` class to `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.Constant.cs` with `AllowedSearchFields = [nameof(VariantImage.FileName), nameof(VariantImage.Alt)]`, `AllowedSortFields = [nameof(VariantImage.Position), nameof(VariantImage.CreatedAtUtc)]`, `AllowedFilterFields = [nameof(VariantImage.Type), nameof(VariantImage.ContentType), nameof(VariantImage.DimensionsUnit)]`, declared as `public static readonly string[]` inside `public static class Query` (string[] style matching the file's sibling constants). | |  |

### Implementation Phase 2

- GOAL-002: Wire full querying into the three EF-native candidate handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002 | Rewrite `service/Api/src/Module/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.cs` `Handle`: add `using Shared.Operational.Persistence.Specifications.Sorting;`, drop the now-redundant `using ...Specifications.Paging;` and `.Paging.Extensions;` lines, replace the `PageModelExtensions.FromValues` block with `ParseAll(StockItemConstant.Query.AllowedFilterFields/SearchFields/SortFields.ToHashSet(StringComparer.OrdinalIgnoreCase))` + `IsFailure` guard, then `dbContext.Set<StockItem>().AsNoTracking().ApplyQuerying(parsing.Value, defaultSortClauses: [new SortClause { Field = nameof(StockItem.Id) }]).ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken)`. Preserve the `// Contract:` and `// Load:` comments. | |  |
| TASK-003 | Rewrite `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.cs` `Handle`: add `using Shared.Operational.Persistence.Specifications.Sorting;`, replace the `PageModelExtensions.FromValues` block with `ParseAll(VariantConstant.Query.AllowedFilterFields/SearchFields/SortFields.ToHashSet(StringComparer.OrdinalIgnoreCase))` + `IsFailure` guard; keep the four `Include`/`ThenInclude` chains and the `Where(x => x.ProductId == query.ProductId && !x.IsDeleted)` predicate; remove `.OrderBy(x => x.Position)`; call `.ApplyQuerying(parsing.Value, defaultSortClauses: [new SortClause { Field = nameof(Variant.Position) }])` before `.ToPagedOrAllAsync(parsing.Value, x => x.MapToDetail<Response>(), cancellationToken)`. Preserve the `// Contract:` and `// Load:` comments. | |  |
| TASK-004 | Rewrite `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.cs` `Handle`: add `using Shared.Operational.Persistence.Specifications.Sorting;`, replace the `PageModelExtensions.FromValues` block with `ParseAll(VariantImageConstant.Query.AllowedFilterFields/SearchFields/SortFields.ToHashSet(StringComparer.OrdinalIgnoreCase))` + `IsFailure` guard; keep `.Where(x => x.VariantId == query.VariantId)`; remove `.OrderBy(x => x.Position)`; call `.ApplyQuerying(parsing.Value, defaultSortClauses: [new SortClause { Field = nameof(VariantImage.Position) }])` before `.ToPagedOrAllAsync(parsing.Value, x => x.MapToDetail<VariantImageDetailResponse>(), cancellationToken)`. Preserve the `// Contract:` and `// Filter:` comments. | |  |

### Implementation Phase 3

- GOAL-003: Add querying tests to the three upgraded handlers' unit test suites.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.Tests.cs`: add a filter test (seed items with distinct `Backorderable` values, query `new Parameters { Filter = "Backorderable=false" }`, assert only matching items returned) and a sort test (`new Parameters { Sort = ["CountOnHand:desc"] }`, assert descending order). | |  |
| TASK-006 | `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Tests.cs`: add a sort test (`Sort = ["Position:desc"]` within a seeded product, assert order) and a disallowed-field rejection test (`Sort = ["NonExistent:asc"]` → `result.IsSuccess.Should().BeFalse()`). | |  |
| TASK-007 | `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Tests.cs`: add a filter test (`Filter = "Type=Default"` over seeded images with distinct `Type`) and a sort test (`Sort = ["Position:desc"]`). | |  |
| TASK-008 | In each of the three test files, add a disallowed-filter-field rejection test (e.g. `Filter = "NonExistent=1"` → `result.IsSuccess.Should().BeFalse()`) verifying SEC-001 enforcement. | |  |

### Implementation Phase 4

- GOAL-004: Verify and commit.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Run `dotnet build service/Api/src/Api/Api.csproj --no-restore` — must report `Build succeeded. 0 Warning(s), 0 Error(s)`. | |  |
| TASK-010 | Run focused tests with `--filter-class` (NOT `--filter "FullyQualifiedName~..."`, which silently runs zero tests): `Module.UnitTests.Inventory.Features.Admin.StockItems.GetAll.GetAllStockItemsTests`, `Module.UnitTests.Catalog.Features.Admin.Products.Variants.List.ListVariantsByProductTests`, `Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.ListByVariant.ListVariantImagesTests` — all must pass. | |  |
| TASK-011 | Run `bash scripts/check-feature-conventions.sh` and confirm **no new** AC-001/002/003/005 violations vs. the pagedresult baseline recorded in the SDD ledger. | |  |
| TASK-012 | Stage by explicit paths (never `git add -A`): the 4 source files + 3 test files. Commit message: `refactor(querying): add full filter/search/sort to EF-native list endpoints`. Verify `git status` shows exactly 7 staged files and the working tree is otherwise clean. | |  |

## 3. Alternatives

- **ALT-001**: Upgrade the in-memory assignment handlers (`GetProductOptionTypes`, `GetProductClassifications`, `GetVariantOptionValues`, `GetUserRoles`) to full querying. **Rejected** — these load the full set to compute `IsAssigned`, then page via `items.ToPagedResult(pageModel)`. `ApplyQuerying` is `IQueryable<T>`-only (CON-001), and filtering before materialization would break assignment semantics (the editor needs the whole candidate set).
- **ALT-002**: Build new in-memory (`IEnumerable<T>`) filter/search/sort extensions in `Shared.Operational.Persistence.Specifications.*`. **Rejected for this plan** — a large infrastructure surface with no current consumer; revisit only if assignment lists are proven to need it.
- **ALT-003**: Convert `GetShippingMethods`, `GetLowStockItems`, `GetCartReservations` to EF-projection + `ParseAll` (as `ListShippingRates` does). **Deferred** — `GetShippingMethods` is a small storefront set with low querying value; the other two compute derived values (threshold, TTL) that cannot be pushed to SQL. The `ShippingMethodConstant.Query` whitelist already exists and can be wired later if product needs change.

## 4. Dependencies

- **DEP-001**: Completed `pagedresult-envelope-standardization` (SDD plan `docs/superpowers/plans/2026-07-31-pagedresult-envelope-standardization.md`) — all 15 endpoints now expose `Parameters : QueryingParameters` and `PagedResult<T>` envelopes. This plan runs on branch `pagedresult-envelope-standardization` at HEAD `5b3dccd6`.
- **DEP-002**: Shared querying infrastructure (`QueryingParametersExtensions.ParseAll`, `QueryingModelExtensions.ApplyQuerying`, `SortClause`, whitelist validation error models) — exists and is exercised by `GetPagedOrders`, `GetPagedStockReservations`, `ListShippingRates`.
- **DEP-003**: Existing whitelists `StockItemConstant.Query` and `VariantConstant.Query` — already declared; this plan consumes them as-is. `VariantImageConstant.Query` is added by TASK-001.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.Constant.cs` (modify — add `Query` whitelist)
- **FILE-002**: `service/Api/src/Module/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.cs` (modify — full querying)
- **FILE-003**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.cs` (modify — full querying)
- **FILE-004**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.cs` (modify — full querying)
- **FILE-005**: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.Tests.cs` (modify — add querying tests)
- **FILE-006**: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Tests.cs` (modify — add querying tests)
- **FILE-007**: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Tests.cs` (modify — add querying tests)

## 6. Testing

- **TEST-001**: `GetAllStockItems` — filter (`Backorderable=false`) returns only matching items.
- **TEST-002**: `GetAllStockItems` — sort (`CountOnHand:desc`) returns descending order.
- **TEST-003**: `ListVariantsByProduct` — sort (`Position:desc`) within a seeded product returns descending order.
- **TEST-004**: `ListVariantImages` — filter (`Type=Default`) returns only matching images.
- **TEST-005**: `ListVariantImages` — sort (`Position:desc`) returns descending order.
- **TEST-006**: Disallowed-field rejection (SEC-001) on each handler — `Filter`/`Sort` referencing a non-whitelisted field yields `result.IsSuccess.Should().BeFalse()`.
- **TEST-007**: Regression — existing paging tests for all three handlers still pass unchanged, and full `Module.UnitTests` build/tests remain green.

## 7. Risks & Assumptions

- **RISK-001**: Applying `ApplyQuerying` while an explicit `.OrderBy(...)` remains on the same query would cause EF to throw on a second `OrderBy`. Mitigated by PAT-002 (remove explicit `OrderBy`, pass `defaultSortClauses`); TASK-002/003/004 are explicit about this.
- **RISK-002**: The InMemory EF provider may not translate every search/filter expression the same as Npgsql. Mitigation: tests exercise simple predicates only (boolean/string equality, single-field sort); search behavior is not asserted in unit tests — covered by the existing integration-test suite contract.
- **RISK-003**: Whitelist field names must match actual entity property names — `nameof(...)` is used throughout to fail at compile time rather than runtime.
- **ASSUMPTION-001**: In-memory assignment/computed handlers (`GetProductOptionTypes`, `GetProductClassifications`, `GetVariantOptionValues`, `GetUserRoles`, `GetLowStockItems`, `GetCartReservations`, `GetShippingMethods`, `GetStockSummary`) remain paging-only and are out of scope (documented in ALT-001/ALT-003).
- **ASSUMPTION-002**: No current SPA or `.http` client sends `filter`/`search`/`sort` to the three upgraded endpoints today, so this is purely additive and backward compatible.
- **ASSUMPTION-003**: The existing `StockItemConstant.Query` and `VariantConstant.Query` whitelists are authoritative and require no changes.

## 8. Related Specifications / Further Reading

- SDD plan (predecessor): `docs/superpowers/plans/2026-07-31-pagedresult-envelope-standardization.md`
- SDD ledger (baseline AC failures + review records): `.superpowers/sdd/2026-07-31-pagedresult-envelope-standardization/progress.md`
- Querying infrastructure: `service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.cs`, `Querying.Model.cs`, `Querying.Model.ApplyExtensions.cs`, `Querying.Parameters.Extensions.cs`
- Sorting infrastructure: `service/Api/src/Shared/Operational/Persistence/Specifications/Sorting/Extensions/SortModelEfCoreExtensions.cs`
- Reference handlers: `GetPagedOrders.cs`, `GetPagedStockReservations.cs`, `ListShippingRates.cs`
