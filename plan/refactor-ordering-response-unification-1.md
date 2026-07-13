---
goal: Eliminate per-feature scalar response types — unify all Cart/Order/LineItem responses under shared model bases
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
status: 'Completed'
tags: [refactor, ordering, models, unification, response-standardization]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Replace 7 standalone per-feature scalar Response types with the shared model hierarchy. Every Cart feature returns `CartDetailResponse` (with Items). Every Admin Order feature returns `OrderDetailResponse`. Every LineItem feature returns `LineItemResponse`. Fix `CreateCart` which currently returns the wrong base (`OrderDetailResponse` instead of `CartDetailResponse`).

**Before:** 7 features return bespoke scalar types like `{ LineItemId }`, `{ Id, ItemCount }`, `{ Id, Status }`
**After:** All features return rich shared response types via mappers

## 1. Requirements & Constraints

- **REQ-001**: Eliminate ALL standalone (non-inherited) `sealed record Response` types — every feature Response MUST inherit from a shared base
- **REQ-002**: Cart features return `CartDetailResponse` (6 scalar fields + `List<CartItem> Items`) — never scalars
- **REQ-003**: Admin Order features return `OrderDetailResponse` (28 fields) — never scalars
- **REQ-004**: All feature responses use shared mappers — never inline `new Response { ... }`
- **REQ-005**: Build: 0 warnings, 0 errors (`TreatWarningsAsErrors=true`)
- **REQ-006**: All 2404 Module.UnitTests pass
- **REQ-007**: No cross-module `Catalog` references in mapper signatures (architecture test)
- **CON-001**: Must not change endpoint routes, HTTP methods, or URI templates
- **CON-002**: Must not change domain entity (`Order`, `LineItem`, `Variant`) structure
- **CON-003**: Shared base records (`CartDetailResponse`, `OrderDetailResponse`, `LineItemResponse`) can have new properties added but none removed
- **CON-004**: `ListCustomerOrders` different — must NOT change (leaner projection, frontend-safe)
- **PAT-001**: All responses constructed via `entity.MapToDetail<T>()` or `entity.MapToDetailWithItems<T>(variantNames)`
- **PAT-002**: Variant name enrichment uses `Dictionary<Guid, string>` to avoid cross-module `Variant` in mapper

## 2. Implementation Steps

### Implementation Phase 1: Unify Cart feature responses to CartDetailResponse

- GOAL-001: All 3 cart features that return data (AddToCart, AssociateCartWithUser, CreateCart) must return `CartDetailResponse`. All features use `MapToDetailWithItems<Response>()` for consistent Items inclusion.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Change `AddToCart.Response` from `{ Guid LineItemId }` to `sealed record Response : CartDetailResponse`. File: `Storefront/Cart/AddItem/AddToCart.Response.cs`. Delete the LineItemId property; add empty body inheriting CartDetailResponse. | | |
| TASK-002 | Refactor `AddToCart.cs` handler: add variant name enrichment query (`dbContext.Set<Variant>().Where(v => ...).ToDictionaryAsync(v => v.Id, v => v.Sku ?? "")`), replace both `new Response { LineItemId = ... }` return sites (line ~121 and ~142) with `cart.MapToDetailWithItems<Response>(variantNames)`. Change `ICommand<Response>` if needed. Add `using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;` and `using Module.Ordering.Features.Storefront.Cart.Shared.Models;`. | | |
| TASK-003 | Change `AssociateCartWithUser.Response` from `{ Guid Id, int ItemCount }` to `sealed record Response : CartDetailResponse`. File: `Storefront/Cart/AssociateCart/AssociateCartWithUser.Response.cs`. Delete the 2 properties; add empty body. | | |
| TASK-004 | Refactor `AssociateCartWithUser.cs` handler: add variant name enrichment query, replace `new Response { Id = ..., ItemCount = ... }` (line ~62) with `targetOrder.MapToDetailWithItems<Response>(variantNames)`. Add required usings. | | |
| TASK-005 | Change `CreateCart.Response` from `: OrderDetailResponse` (wrong!) to `sealed record Response : CartDetailResponse`. File: `Storefront/Cart/CreateCart/CreateCart.Response.cs`. | | |
| TASK-006 | Refactor `CreateCart.cs` handler: change `using Module.Ordering.Features.Admin.Orders.Shared.Mappings;` to `using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;`. Add optional variant name enrichment query (new cart has empty items, so lookup is trivial). Replace all `order.MapToDetail<Response>()` calls with `order.MapToDetailWithItems<Response>(variantNames)` (2 call sites: line ~30 and ~40). | | |
| TASK-007 | Build `dotnet build service/Api/src/Module/ --no-restore` — fix any compile errors. | | |

### Implementation Phase 2: Unify Admin Order feature responses to OrderDetailResponse

- GOAL-002: All 4 admin order mutation features (AddOrderLineItem, ApproveOrder, ResumeOrder, UpdateLineItem) return full `OrderDetailResponse` instead of scalar fields. None need re-fetch — the tracked `Order` entity already holds all scalars.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Change `AddOrderLineItem.Response` from `{ Guid Id, Guid VariantId, int Quantity, decimal Total }` to `sealed record Response : OrderDetailResponse`. File: `Admin/Orders/AddLineItem/AddOrderLineItem.Response.cs`. Delete the 4 properties; add empty body. | | |
| TASK-009 | Refactor `AddOrderLineItem.cs` handler: replace `new Response { Id = ..., VariantId = ..., Quantity = ..., Total = ... }` (line ~38) with `order.MapToDetail<Response>()`. Add `using Module.Ordering.Features.Admin.Orders.Shared.Mappings;`. The `order` variable is EF-tracked with all scalar properties. | | |
| TASK-010 | Change `ApproveOrder.Response` from `{ Guid Id, Guid? ApprovedById, DateTimeOffset? ApprovedAtUtc }` to `sealed record Response : OrderDetailResponse`. File: `Admin/Orders/Approve/ApproveOrder.Response.cs`. Delete the 3 properties; add empty body. | | |
| TASK-011 | Refactor `ApproveOrder.cs` handler: replace `new Response { Id = ..., ApprovedById = ..., ApprovedAtUtc = ... }` (line ~29) with `order.MapToDetail<Response>()`. Add admin mappings using. | | |
| TASK-012 | Change `ResumeOrder.Response` from `{ Guid Id, OrderStatus Status }` to `sealed record Response : OrderDetailResponse`. File: `Admin/Orders/Resume/ResumeOrder.Response.cs`. Delete the 2 properties; add empty body. | | |
| TASK-013 | Refactor `ResumeOrder.cs` handler: replace `new Response { Id = ..., Status = ... }` (line ~39) with `order.MapToDetail<Response>()`. Add admin mappings using. | | |
| TASK-014 | Change `UpdateOrderLineItem.Response` from `{ Guid Id, int Quantity, decimal Total }` to `sealed record Response : OrderDetailResponse`. File: `Admin/Orders/UpdateLineItem/UpdateOrderLineItem.Response.cs`. Delete the 3 properties; add empty body. | | |
| TASK-015 | Refactor `UpdateOrderLineItem.cs` handler: replace `new Response { Id = ..., Quantity = ..., Total = ... }` (line ~46) with `order.MapToDetail<Response>()`. Add admin mappings using. | | |
| TASK-016 | Build `dotnet build service/Api/src/Module/ --no-restore` — fix any compile errors. | | |

### Implementation Phase 3: Verify ListCustomerOrders stays as-is

- GOAL-003: Confirm `ListCustomerOrders.Response` should NOT change — its 5-field projection is leaner and safer for storefront.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Verify `ListCustomerOrders.Response` current state: standalone `sealed record Response { Guid Id, string Number, string Status, decimal Total, DateTimeOffset CreatedAtUtc }`. This is correct — storefront listing needs fewer fields than `OrderListItemResponse` (13 fields) and uses `string Status` (not `OrderStatus` enum). **Skip.** | | |
| TASK-018 | Document in the Response.cs file header WHY this feature is intentionally standalone (storefront lean listing). | | |

### Implementation Phase 4: Final verification

- GOAL-004: Build 0W/0E, all tests pass, zero standalone Response types remain.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Full build: `dotnet build` — 0W/0E. | | |
| TASK-020 | Run unit tests: `dotnet test service/Api/tests/Module.UnitTests` — all 2404 pass. | | |
| TASK-021 | Verify zero standalone non-inherited `Response` types: `rg "sealed record Response\b" -g "*.cs" service/Api/src/Module/Ordering/Features/ \| grep -v " : "` should show ONLY `ListCustomerOrders.Response`. | | |
| TASK-022 | Verify all cart features return `CartDetailResponse`: `rg "sealed record Response" -g "*.cs" service/Api/src/Module/Ordering/Features/Storefront/Cart/ \| grep "Response"` — all must show `: CartDetailResponse`. | | |
| TASK-023 | Verify all admin order features return `OrderDetailResponse`: `rg "sealed record Response" -g "*.cs" service/Api/src/Module/Ordering/Features/Admin/Orders/ \| grep "Response"` — all must show `: OrderDetailResponse` or `: LineItemResponse` or `: OrderListItemResponse`. | | |
| TASK-024 | Architecture test pass: `dotnet test --filter "FullyQualifiedName~ModuleIsolation"` (no cross-module refs). | | |
| TASK-025 | Verify K6 / e2e tests pass if applicable. | | |

## 3. Alternatives

- **ALT-001**: Keep scalar responses and just add a comment saying "you should also GET the full cart/order" — rejected, user explicitly wants rich responses, not half-baked scalars.
- **ALT-002**: Introduce a new shared `CartDetailWithItemsResponse` and keep `CartDetailResponse` items-free — rejected, CartDetailResponse already has Items list (defaults to empty), no new type needed.
- **ALT-003**: Change `ListCustomerOrders` to use `OrderListItemResponse` — rejected, would leak admin fields and change Status from string to enum, breaking storefront frontend.
- **ALT-004**: Have cart mutations NOT include Items (use plain `MapToDetail<T>`) — rejected per user instruction "avoid the Cart with multi Items", they want rich cart responses always.

## 4. Dependencies

- **DEP-001**: `CartMapping.MapToDetailWithItems<T>(Dictionary<Guid, string> variantNames)` — already exists in `Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs`.
- **DEP-002**: `OrderMapping.MapToDetail<T>()` — already exists in `Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs`.
- **DEP-003**: Shared models `CartDetailResponse`, `OrderDetailResponse`, `LineItemResponse` — already exist.
- **DEP-004**: Previous standardization plans complete:
  - `refactor-ordering-endpoint-standardization-1.md` — [FromRoute]/Produces/class→record
  - `refactor-ordering-shared-models-standardization-1.md` — consolidation, validators, mappers

## 5. Files

### Modified Response files (7)
- **FILE-001**: `Storefront/Cart/AddItem/AddToCart.Response.cs` — → `: CartDetailResponse`
- **FILE-002**: `Storefront/Cart/AssociateCart/AssociateCartWithUser.Response.cs` — → `: CartDetailResponse`
- **FILE-003**: `Storefront/Cart/CreateCart/CreateCart.Response.cs` — → `: CartDetailResponse` (was wrongly `: OrderDetailResponse`)
- **FILE-004**: `Admin/Orders/AddLineItem/AddOrderLineItem.Response.cs` — → `: OrderDetailResponse`
- **FILE-005**: `Admin/Orders/Approve/ApproveOrder.Response.cs` — → `: OrderDetailResponse`
- **FILE-006**: `Admin/Orders/Resume/ResumeOrder.Response.cs` — → `: OrderDetailResponse`
- **FILE-007**: `Admin/Orders/UpdateLineItem/UpdateOrderLineItem.Response.cs` — → `: OrderDetailResponse`

### Modified Handler files (7)
- **FILE-008**: `Storefront/Cart/AddItem/AddToCart.cs` — variant lookup + `MapToDetailWithItems<Response>()`
- **FILE-009**: `Storefront/Cart/AssociateCart/AssociateCartWithUser.cs` — variant lookup + `MapToDetailWithItems<Response>()`
- **FILE-010**: `Storefront/Cart/CreateCart/CreateCart.cs` — change using + `MapToDetailWithItems<Response>()`
- **FILE-011**: `Admin/Orders/AddLineItem/AddOrderLineItem.cs` — `order.MapToDetail<Response>()`
- **FILE-012**: `Admin/Orders/Approve/ApproveOrder.cs` — `order.MapToDetail<Response>()`
- **FILE-013**: `Admin/Orders/Resume/ResumeOrder.cs` — `order.MapToDetail<Response>()`
- **FILE-014**: `Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs` — `order.MapToDetail<Response>()`

### Verified (no change needed)
- **FILE-015**: `Storefront/Orders/ListOrders/ListCustomerOrders.Response.cs` — stays as-is (lean storefront list)
- **FILE-016**: `Storefront/Cart/Get/GetCart.Response.cs` — already `: CartDetailResponse` ✅
- **FILE-017**: `Storefront/Orders/Get/ById/GetCustomerOrder.Response.cs` — already `: OrderDetailResponse` ✅

### Possibly affected (check)
- **FILE-018**: `Storefront/Cart/AddItem/AddToCart.Endpoint.cs` — may need `.Produces<>()` update if response type changed
- **FILE-019**: `Storefront/Cart/AssociateCart/AssociateCartWithUser.Endpoint.cs` — same
- **FILE-020**: `Storefront/Cart/CreateCart/CreateCart.Endpoint.cs` — same

## 6. Testing

- **TEST-001**: `dotnet build service/Api/src/Module/` — 0W/0E
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all 2404 pass, 0 failed
- **TEST-003**: Architecture: `dotnet test --filter "FullyQualifiedName~ModuleIsolation"` — 0 cross-module refs
- **TEST-004**: Verify `MapToDetailWithItems<T>()` resolves correctly for `CartDetailResponse` constraint — `Response : CartDetailResponse` satisfies `where T : CartDetailResponse, new()`
- **TEST-005**: Run Admin SPA tests: `cd app/Admin && pnpm run test:unit` (if breakages from new response fields)
- **TEST-006**: Run Store SPA tests: `cd app/Store && pnpm run test:unit` (if breakages from new cart response fields)

## 7. Risks & Assumptions

- **RISK-001**: Changing `AddToCart` response from `{ lineItemId }` to full `CartDetailResponse` breaks frontend clients that expect `{ lineItemId }`. Mitigation: frontend gets more data (backward-compatible), not less. New fields are additive.
- **RISK-002**: `AssociateCartWithUser` response changes from `{ id, itemCount }` to full `CartDetailResponse` — same mitigation as above.
- **RISK-003**: Adding variant name enrichment (extra DB query) to AddToCart, AssociateCartWithUser, CreateCart may slow mutations. Mitigation: this is a standard pattern (GetCart already does it). The variant lookup is batched with `.Where(v => variantIds.Contains(v.Id))`.
- **RISK-004**: Admin `OrderDetailResponse` exposes sensitive fields (PaymentTotal, PaymentState, ShipmentState, etc.) to admin endpoints. Already exposed — these ARE admin endpoints. No change in sensitivity.
- **ASSUMPTION-001**: All tracked `Order` entities in mutation handlers have their scalar properties populated after `SaveChangesAsync()`. True for EF Core default behavior.
- **ASSUMPTION-002**: Frontend TypeScript clients will handle the richer response payloads without breakage. New fields are additive; existing JSON properties are preserved.
- **ASSUMPTION-003**: `ListCustomerOrders` continues to return its existing 5-field projection — no test regressions expected.

## 8. Related Specifications / Further Reading

- [Previous: endpoint standardization plan](./refactor-ordering-endpoint-standardization-1.md)
- [Previous: model consolidation plan](./refactor-ordering-shared-models-standardization-1.md)
- [Architecture doc](../docs/codebase/ARCHITECTURE.md)
- [AGENTS.md](../AGENTS.md) — Rules 1 (Result>), 2 (no cross-module refs), 3 (vertical slices)
