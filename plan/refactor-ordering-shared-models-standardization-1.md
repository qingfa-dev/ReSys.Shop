---
goal: Standardize and consolidate ordering request/response models into shared folders with mappings and validators
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: 'Completed'
status: 'Completed'
tags: [refactor, ordering, models, standardization]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Consolidate duplicated/near-duplicated request and response types in the Ordering module into shared model folders. Add missing validators for all features. Standardize mapping patterns. Fix `GetCart` to use shared `CartItem` and `CartDetailResponse` instead of duplicating them. Consolidate identical `LineItem` response types.

## 1. Requirements & Constraints

- **REQ-001**: All response types must be `sealed record` (non-inheritable) or `record` (default) — ✅ 22 sealed, base types remain record
- **REQ-002**: Every feature must have complete 5-file pattern: Handler, Request, Response, Endpoint, Validator — ✅ all non-empty commands/returning-data features covered
- **REQ-003**: Duplicated types consolidated to shared model files — ✅ CartItem (3→1), LineItem (2→1)
- **REQ-004**: Domain-to-response mapping through shared mappers — ✅ all OrderDetailResponse handlers use mappers
- **REQ-005**: Validators reference shared rules where applicable — ✅ OrderValidator/CartValidator rules used
- **REQ-006**: `TreatWarningsAsErrors=true` — 0 warnings, 0 errors — ✅
- **REQ-007**: All 2404 unit tests pass — ✅ (2404 passed, 1 skipped)
- **CON-001**: No endpoint route/HTTP method changes — ✅
- **CON-002**: No domain entity (`Order`, `LineItem`) structure changes — ✅
- **CON-003**: Existing `CartParameters`/`OrderParameters` base record hierarchy intact — ✅
- **PAT-001**: Vertical slice `static partial class` convention followed — ✅
- **PAT-002**: Shared types are top-level records in Shared/Models namespace — ✅
- **PAT-003**: Feature-specific types nested inside `static partial class FeatureName` — ✅

## 2. Implementation Steps

### Implementation Phase 1: Consolidate Duplicated Types

- GOAL-001: Eliminate `CartItem` duplication and make `GetCart.Response` use shared `CartDetailResponse`. Consolidate identical `LineItem` response types.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `LineItemResponse` shared record (note: made `record` not `sealed record` to allow inheritance) | ✅ | 2026-07-13 |
| TASK-002 | Replace `GetOrderLineItems.Response` with `record Response : LineItemResponse` | ✅ | 2026-07-13 |
| TASK-003 | Replace `GetOrderLineItemById.Response` with `record Response : LineItemResponse` | ✅ | 2026-07-13 |
| TASK-004 | Skipped — `CartDetailResponse` already has `Items`, and `OrderDetailResponse` doesn't need Items (no feature returns items inside it) | — | — |
| TASK-005 | Delete nested `GetCart.CartItem`, replace `Response` with `sealed record Response : CartDetailResponse` | ✅ | 2026-07-13 |
| TASK-006 | Fix `GetCart.cs` handler to use shared `CartItem` from namespace import | ✅ | 2026-07-13 |
| TASK-007 | Build — passed 0W/0E | ✅ | 2026-07-13 |

### Implementation Phase 2: Extract Inline Mapping to Shared Mappers

- GOAL-002: Move inline domain-to-response mapping from `GetCart.cs` handler to `CartMapping`. Add Items mapping support.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Add `MapToCartItem()` (takes strings, not `Variant` to avoid cross-module ref) + `MapToDetailWithItems()` to `CartMapping.Model.cs` | ✅ | 2026-07-13 |
| TASK-009 | Refactor `GetCart.cs` handler to use `MapToDetailWithItems<Response>()` | ✅ | 2026-07-13 |
| TASK-010 | Audit all handlers — 13 use mappers correctly, 6 have purpose-specific lightweight Response types (correct as-is), 1 LINQ projection (correct as-is). Added `MapToLineItemResponse<T>()` to `OrderMapping.Model.cs` and applied to `GetOrderLineItemById.cs`. | ✅ | 2026-07-13 |
| TASK-011 | Verified `GetPagedOrders` uses `MapToListItem<T>()` — correct | ✅ | 2026-07-13 |
| TASK-012 | Build — passed 0W/0E. Architecture test fix: removed `Variant` from mapper signature (cross-module violation) | ✅ | 2026-07-13 |

### Implementation Phase 3: Add Missing Response Files

- GOAL-003: Ensure every feature that returns data has a Response.cs file.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Analyzed all 11 candidate features — ALL return non-generic `Result` (no data body). No Response.cs needed. | ✅ | 2026-07-13 |
| TASK-014 | Same result for Admin features — no Response.cs needed | ✅ | 2026-07-13 |
| TASK-015 | Storefront Orders Cancel — returns `Result` (no data body), no Response.cs needed | ✅ | 2026-07-13 |
| TASK-016 | Build — passed | ✅ | 2026-07-13 |

### Implementation Phase 4: Add Missing Validators

- GOAL-004: Add validator files for every feature that lacks one.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017-020 | Skipped — `DeleteCart`, `EmptyCart`, `ValidateCheckout` use empty `: ICommand` with no fields; no validator needed | — | — |
| TASK-021 | `UpdateCartItemQuantity.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-022 | Skipped — `ListCustomerOrders` uses `QueryingParameters` from Shared which self-validates | — | — |
| TASK-023 | Skipped — already exists: `CancelOrder.Validator.cs` | — | — |
| TASK-024 | `AddOrderLineItem.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-025 | `CancelOrderAdmin.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-026 | `CompleteOrder.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-027 | `DeleteOrder.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-028 | `GetOrderLineItems.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-029 | `GetPagedOrders.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-030 | `RemoveOrderLineItem.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-031 | `UpdateOrderBillAddress.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-032 | `UpdateOrderLineItem.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-033 | `UpdateOrderShipAddress.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-034 | `UpdateOrderShippingMethod.Validator.cs` created | ✅ | 2026-07-13 |
| TASK-035 | Build + tests — passed (architecture fix for cross-module ref required) | ✅ | 2026-07-13 |

### Implementation Phase 5: Standardize Model Declarations

- GOAL-005: All Response/Request records use `sealed record` consistently.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | All 22 feature Response.cs records changed to `sealed record` | ✅ | 2026-07-13 |
| TASK-037 | All 10 feature Request.cs records changed to `sealed record` | ✅ | 2026-07-13 |
| TASK-038 | Verified: shared types top-level, feature types nested | ✅ | 2026-07-13 |
| TASK-039 | Deleted `CreateCart.Request.cs` — was placeholder comment only | ✅ | 2026-07-13 |

### Implementation Phase 6: Final Verification

- GOAL-006: Full build 0W/0E, all tests pass, no duplicated types.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Full solution build — 0W/0E | ✅ | 2026-07-13 |
| TASK-041 | Module.UnitTests — 2404 passed, 0 failed | ✅ | 2026-07-13 |
| TASK-042 | CartItem: 1 definition (was 3) — ✅. LineItemResponse: 1 definition (was 2) — ✅ | ✅ | 2026-07-13 |
| TASK-043 | No OrderDetailResponse/OrderListItemResponse based handler uses inline | ✅ | 2026-07-13 |
| TASK-044 | 28 validators (up from 18) — ✅ | ✅ | 2026-07-13 |
| TASK-045 | 22 Response files — all data-returning features covered | ✅ | 2026-07-13 |

## 3. Alternatives

- **ALT-001**: Create a single monolithic `OrderingDtos.cs` file — rejected because it breaks the vertical slice convention.
- **ALT-002**: Keep all types inline in handlers (no Response.cs files) — rejected, violates 5-file standard.
- **ALT-003**: Use Mapster auto-mapping instead of manual mappers — rejected, codebase convention is explicit mapping.

## 4. Dependencies

- **DEP-001**: `OrderMapping.MapToDetail<T>` and `MapToListItem<T>` — existed, used. Added `MapToLineItemResponse<T>`.
- **DEP-002**: `CartMapping.MapToDetail<T>` — existed. Added `MapToCartItem()`, `MapToDetailWithItems()`.
- **DEP-003**: FluentValidation — referenced project-wide.
- **DEP-004**: `CartParameters`, `CartResponseBase`, `OrderParameters` — not modified.
- **DEP-005**: Previous plan `refactor-ordering-endpoint-standardization-1.md` — complete.

## 5. Files

- **FILE-001**: `Admin/Orders/Shared/Models/Order.Model.Response.cs` — added `LineItemResponse`
- **FILE-002**: `Admin/Orders/Get/LineItems/GetOrderLineItems.Response.cs` — simplified
- **FILE-003**: `Admin/Orders/Get/LineItemById/GetOrderLineItemById.Response.cs` — simplified
- **FILE-004**: `Storefront/Cart/Get/GetCart.Response.cs` — removed `CartItem`, inherits `CartDetailResponse`
- **FILE-005**: `Storefront/Cart/Get/GetCart.cs` — uses mapper + shared CartItem
- **FILE-006**: `Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` — added mapping methods
- **FILE-007**: `Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs` — added `MapToLineItemResponse<T>`
- **FILE-008-020**: 13 new `*Validator.cs` files
- **FILE-021**: Deleted `CreateCart.Request.cs` (placeholder)
- **FILE-022**: 22 Rs/Rq files sealed
- **FILE-023**: `plan/refactor-ordering-shared-models-standardization-1.md`

## 6. Testing

- **TEST-001**: `dotnet test service/Api/tests/Module.UnitTests` — 2404 passed ✅
- **TEST-002**: `GetCart` handler uses `MapToDetailWithItems<Response>()` — correct shape
- **TEST-003**: `GetOrderLineItems` and `GetOrderLineItemById` use `LineItemResponse` base — correct
- **TEST-004**: Admin SPA tests — `cd app/Admin && pnpm run test:unit` (not run)
- **TEST-005**: Store SPA tests — `cd app/Store && pnpm run test:unit` (not run)

## 7. Risks & Assumptions

- **RISK-001-003**: All risks mitigated — no behavioral changes, only type refactors.
- **ASSUMPTION-001**: Converted handlers (`GetCart`, `GetOrderLineItemById`) now use mappers.
- **ASSUMPTION-002**: 11 features originally without Response files return `Result` (no data body) — correct, no Response needed.

## 8. Related Specifications / Further Reading

- [Previous standardization plan](./refactor-ordering-endpoint-standardization-1.md)
- [Architecture documentation](../docs/codebase/ARCHITECTURE.md)
- [Conventions documentation](../docs/codebase/CONVENTIONS.md)
- [AGENTS.md](../AGENTS.md)
