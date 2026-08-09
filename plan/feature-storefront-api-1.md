---
goal: Fix storefront API defects, consolidate inventory reservation, add payment status + card-save, add catalog taxon-browse endpoints
version: 1.0
date_created: 2026-08-09
last_updated: 2026-08-09
owner: ReSys.Shop team
status: Complete (Phases 1–4; Phase 5 excluded, card-save deferred)
tags: feature, storefront, api, inventory, payment, catalog, refactor
---

# Introduction

![Status: Complete](https://img.shields.io/badge/status-Complete-green)

ReSys.Shop storefront API currently exposes 78 HTTP endpoints across 8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping), all wired handler+validator, ~80% feature-complete. This plan covers Phases 1–4 of the completion work: (1) fix defects, (2) consolidate inventory stack, (3) add payment status endpoint + defer card-save, (4) add catalog taxon-browse-permalink endpoints. Phase 5 (Identity gaps + route-base unification + cross-module cleanup) explicitly excluded per requester.

## 1. Requirements & Constraints

- **REQ-001**: `DELETE /api/storefront/cart/reserve/{reservationId:guid}` must bind exactly one endpoint (currently `ReleaseCartReservation.Endpoint.cs` and `ReleaseSingleReservation.Endpoint.cs` both register it — ambiguous match)
- **REQ-002**: Cart stock reservation TTL must be single source of truth; old REST `CartReservations/Reserve` default (15) and new `ReserveCartStock.Command` default (30) must converge to one value
- **REQ-003**: `ListVisualSearchModels` endpoint must not declare unreachable status codes (`Produces(400)` but handler returns `Unexpected`, no 400 path)
- **REQ-004**: Inventory must have one canonical reservation stack; old REST `CartReservations/*` and new command stack (`ReserveCartStockCommand`, `ConsumeCartStockReservationsCommand`, `ReleaseCartStockReservationsCommand`) must be reconciled
- **REQ-005**: Payment status of an order must be queryable over HTTP (currently `GetPaymentForCheckoutQuery` in-process only; no SPA polling endpoint)
- **REQ-006**: Card-save attach flow (`SetupIntent`) must persist the saved payment method over HTTP — **DEFERRED**: no `SavedPaymentMethod` EF entity + no SPA caller; would need migration (violates ASSUMPTION-003); cut from Phase 3, tracked as follow-up
- **REQ-007**: Catalog must support browse-by-taxon-permalink: `GET /api/storefront/taxons/{permalink}` and `GET .../taxons/{permalink}/products`
- **SEC-001**: Payment status must scope by authenticated owner / cart ownership; no cross-user disclosure
- **SEC-002**: PaymentMethods/Save must validate method active + non-deleted; no save of inactive or foreign method
- **CON-001**: Every C# feature action is `static partial class` in `Features/{Storefront}/{Feature}/{Action}/` with Handler, Request, Response, Endpoint, Validator (read-only queries may omit Request/Validator)
- **CON-002**: `TreatWarningsAsErrors=true` — any warning fails `dotnet build`
- **CON-003**: Route constants live in `{Module}Feature.Storefront.cs` under each module's `Features/Shared/`; inline route strings forbidden in endpoints
- **CON-004**: No new cross-module `using Module.X...` introducing violations beyond the existing 38 baseline
- **GUD-001**: `.ToResult()`/`result.ToResult()` extension pattern for endpoints; `Result<T>`/`Result` returns, no exceptions for domain errors
- **GUD-002**: TTL constant placed in `InventoryFeature.Storefront.CartReservations` as `TtlMinutesDefault = 30`
- **PAT-001**: New endpoints mirror adjacent siblings (e.g. `Product Endpoint` ricina pattern; `TaxonStore.Endpoint`); Carter `ICarterModule` for registration
- **PAT-002**: Payment status endpoint is read-only query wrapping existing `GetPaymentForCheckoutQuery` handler through `ISender`

## 2. Implementation Steps

### Implementation Phase 1: Fix defects

- **GOAL-001**: Remove duplicate inventory DELETE route, converge TTL to single constant, align VisualSearchModels contract

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/ReleaseSingle/` files (`ReleaseSingleReservation.Endpoint.cs`, `ReleaseSingleReservation.cs`, `ReleaseSingleReservation.Command.cs`); keep `ReleaseCartReservation` (token-scoped) | ✅ 2026-08-09 | |
| TASK-002 | Add `TtlMinutesDefault = 30` constant in `InventoryFeature.Storefront.CartReservations` at `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs` | ✅ 2026-08-09 | |
| TASK-003 | Update `CartReservations/Reserve/ReserveCartStock.Request.cs:10` TtlMinutes default 15 → `InventoryFeature.Storefront.CartReservations.TtlMinutesDefault`; check the new `ReserveCartStock.Request` structure | ✅ 2026-08-09 | |
| TASK-004 | Update `ReserveCartStock.Command.cs` `TtlMinutes` init 30 → same constant | ✅ 2026-08-09 | |
| TASK-005 | Update `Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs:69` `CartReservationTtlMinutes = 30` → same constant | ✅ 2026-08-09 | |
| TASK-006 | Remove `.Produces(StatusCodes.Status400BadRequest)` from `Catalog/.../VisualSearchModels/ListVisualSearchModels.Endpoint.cs` (handler has no 400 path) | ✅ 2026-08-09 | |

### Implementation Phase 2: Consolidate inventory

- **GOAL-002**: Reconcile old REST reservation API and new command stack into one canonical path, no behavior regression

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create inventory consolidation decision: both stacks write same `StockReservation` aggregate (observed duplication: `CartReservations/Reserve` TTL 15 vs new `ReserveCartStockCommand` TTL 30); after Phase 1 TTL fix, keep both edges wired — old REST still called by `app/Store#cartReservationApi.ts` + legacy; new command stack used in-process by `AddToCart`, `CreateIntentFromCart`, `CreatePaymentIntent`, `CreateOrderFromCart` | ✅ 2026-08-09 | |
| TASK-008 | `scripts/check-feature-conventions.sh` pass — no dangling references (baseline 22 pre-existing FAILs held, no new) | ✅ 2026-08-09 | |
| TASK-009 | `dotnet build` green (warnings-as-errors) | ✅ 2026-08-09 | |

### Implementation Phase 3: Payment status + card-save

- **GOAL-003**: Add storefront payment status polling + card-save persist endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Add route constant as in `PaymentFeature.Storefront.cs`: `Payment/Status` → `api/storefront/payment/status/{orderId:guid}` (Save deferred — no entity/migration) | ✅ 2026-08-09 | |
| TASK-011 | Implement `Features/Storefront/Payment/Status/` — `GetPaymentStatus` static-partial Handler+Query+Response+Endpoint; wraps `GetPaymentForCheckoutQuery` + ownership gate via `ICurrentUser` vs order `UserId`; `RequireAuthorization` | ✅ 2026-08-09 | |
| TASK-012 | Implement `Features/Storefront/PaymentMethods/Save/` — **DEFERRED**: needs new SavedPaymentMethod EF entity + migration; unblocks under Phase 5 follow-up | ❌ deferred | |
| TASK-013 | Unit-test `Status`: correct owner → returns amount/completed; wrong owner → 404/not-found (5 tests in `GetPaymentStatusTests.cs`) | ✅ 2026-08-09 | |
| TASK-014 | Unit-test `Save` — deferred with TASK-012 | ❌ deferred | |

### Implementation Phase 4: Catalog taxon browse

- **GOAL-004**: Add browse-by-taxon-permalink endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | In `CatalogFeature.Storefront.cs` (Classifications.Taxons) add route constants: `Taxons.Permalink` → `api/storefront/taxons/{permalink}`; `Taxons.Products` → `api/storefront/taxons/{permalink}/products` | ✅ 2026-08-09 | |
| TASK-016 | Implement `Products/Get/ByTaxonPermalink/` — handler resolves taxon by permalink (breadcrumb chain), filters `Product.Classifications.Any(c => c.TaxonId == taxon.Id)` paged; endpoint `GET`; returns `StoreProductListItemResponse`; follows `GetStorefrontProducts` pattern | ✅ 2026-08-09 | |
| TASK-017 | Unit tests `GetProductsByTaxonPermalink.Tests.cs` (3) — valid permalink → paged products; unknown permalink → 404; products outside taxon excluded. Also `GetTaxonByPermalink.Tests.cs` (3): breadcrumb+children, unknown → not-found | ✅ 2026-08-09 | |

## 3. Alternatives

- **ALT-001**: Unify all endpoints to single `api/storefront` base (currently Catalog/Inventory/Ordering/Payment/Shipping use it; Identity/Location/Profile use `api/store`) — considered, deferred to Phase 5 exclusión
- **ALT-002**: Delete legacy REST inventory reservation stack outright instead of keeping both wired — rejected: `app/Store` current SPA + `ApiTests` still call old routes; TTL convergence + doc-comment chosen as lower-risk reconcile
- **ALT-003**: Payment status via WebSocket/push instead of HTTP polling — rejected: existing in-process query + simple SPA polling sufficient; no infra change

## 4. Dependencies

- **DEP-001**: `GetPaymentForCheckoutQuery` (in-process, `Payment` module) — new Status endpoint wraps it
- **DEP-002**: `GetCartForCheckoutQuery` (in-process, `Ordering`) — ownership gate for Status
- **DEP-003**: `PaymentMethod` domain + `GatewayRegistry` — card-save validation/trigger
- **DEP-004**: `Taxon` domain permalink property + taxon→product association — needed for Phase 4; verify property name exists in `Domain/Classifications/Taxons/`

## 5. Files

- **FILE-001**: `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs` — TTL constant
- **FILE-002**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/ReleaseSingle/` (3 files) — delete
- **FILE-003**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Request.cs` — TTL default
- **FILE-004**: `service/Api/src/Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.Command.cs` — TTL init
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — TTL const
- **FILE-006**: `service/Api/src/Module/Catalog/Features/Storefront/Products/VisualSearchModels/ListVisualSearchModels.Endpoint.cs` — remove produces 400
- **FILE-007**: `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Storefront.cs` — new route constants
- **FILE-008**: `service/Api/src/Module/Payment/Features/Storefront/Payment/Status/` — new dir (4-5 files)
- **FILE-009**: `service/Api/src/Module/Payment/Features/Storefront/PaymentMethods/Save/` — new dir (4-5 files)
- **FILE-010**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs` — taxon permalink route constants
- **FILE-011**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/ByTaxonPermalink/` — new dir (`GetProductsByTaxonPermalink.cs`, `.Parameters.cs`, `.Response.cs`, `.Endpoint.cs`)
- **FILE-012**: `service/Api/src/Module/Catalog/Features/Storefront/Classifications/Taxons/GetByPermalink/` — new dir (`GetTaxonByPermalink.cs`, `.Response.cs`, `.Endpoint.cs`)
- **FILE-013**: `service/Api/tests/Module.UnitTests/.../GetPaymentStatusTests.cs`, `GetTaxonByPermalink.Tests.cs`, `GetProductsByTaxonPermalink.Tests.cs` — new unit tests (11 total)

## 6. Testing

- **TEST-001**: `scripts/check-feature-conventions.sh` — no orphan features after deletes
- **TEST-002**: `dotnet build` — zero warnings
- **TEST-003**: `dotnet test service/Api/tests/Module.UnitTests` — 2625 total; new tests pass; only 3 pre-existing failures (EmbeddingOrchestrator ×2 + `ModuleIsolationTests` cross-ref 38 baseline)
- **TEST-004**: Payment Status GET — authenticated owner returns correct status, foreign owner → not-found
- **TEST-005**: Payment card-save — DEFERRED (no entity/migration/SPA caller)
- **TEST-006**: Catalog taxon permalink — valid permalink returns products (3 t), unknown → 404; taxon breadcrumb/children (3 t)
- **TEST-007**: `cd app/Store && pnpm run lint && pnpm run test:unit` — SPA unchanged (not run; no SPA changes)
- **TEST-008**: `scripts/check-cross-module-refs.sh` — at 38 baseline ✔

## 7. Risks & Assumptions

- **RISK-001**: TTL change 15→30 lengthens cart holds; small oversell window increase — mitigated by serializable transaction + per-sum checks in `ReserveCartStock`
- **RISK-002**: Taxons permalink property name may differ from assumed in Domain — verifying in Phase 4 step; adjust property access if wrong
- **RISK-003**: Deleting `ButtonOne.ReleaseSingleReservation` may break `ApiTests/*.http` referencing it — verify in Phase 1 build
- **RISK-004**: `app/Store` legacy `cartTokenAsync` calls `POST /api/storefront/cart/reserve` may regress with TTL change — verify `app/Store/src/features/industry/services/cartReservationApi.ts` sends TTL
- **ASSUMPTION-001**: Old REST reservation API remains wired in phase 2, no deletion, per user decision to keep `ReleaseCartReservation`
- **ASSUMPTION-002**: Phase 5 (Identity architecture gaps, route-base unification, cross-module refs) excluded — separate follow-up plan
- **ASSUMPTION-003**: No new EF entities/migrations needed in Phases 1–3 (endpoints re-read existing aggregates); Phase 4 relies on existing taxon/product relations

## 8. Related Specifications / Further Reading

- `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs`
- `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Storefront.cs`
- `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Store.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`
- `app/Store/src/shared/constants/api.ts` (base-path split `api/store` vs `api/storefront`)
- `AGENTS.md` (verification commands)