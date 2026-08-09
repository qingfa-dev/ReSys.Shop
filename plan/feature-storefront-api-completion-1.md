---
goal: Fix defects, consolidate inventory reservation, complete payment status and catalog taxon-browse surfaces, and add payment-method status across storefront API
version: 1.0
date_created: 2026-08-09
last_updated: 2026-08-09
owner: ReSys.Shop platform team
status: In progress
tags: feature, storefront, api, inventory, payment, catalog
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

Review of `service/Api/src/Module/*/Features/Storefront/` found 78 wired endpoints across 8 modules, ~80% feature-complete. This plan covers Phases 1–4 of the completion work: (1) fix defects (duplicate inventory DELETE route, TTL divergence, VisualSearchModels contract mismatch), (2) consolidate inventory reservation stacks onto a single TTL + cleaned edge, (3) complete payment surface (status polling exposed over HTTP, card-save flow), (4) add catalog taxon-browse-by-permalink endpoints. Phase 5 (Identity gaps + route/cross-module cleanup) explicitly excluded per requester.

## 1. Requirements & Constraints

- **REQ-001**: `DELETE api/storefront/cart/reserve/{reservationId:guid}` must bind to exactly one endpoint (currently both `ReleaseCartReservation.Endpoint.cs` and `ReleaseSingleReservation.Endpoint.cs` register it — ambiguous)
- **REQ-002**: Cart stock reservation TTL must be a single value (currently `15` in `CartReservations/Reserve/ReserveCartStock.Request.cs:10`, `30` in `ReserveCartStock.Command.cs:7`, hardcoded `30` in `AddToCart.cs:69`)
- **REQ-003**: `ListVisualSearchModels` endpoint must declare only reachable HTTP status codes (currently `.Produces(400)` but handler returns `Unexpected` with no 400 path)
- **REQ-004**: Legacy REST reservation API (`POST/GET/DELETE /api/storefront/cart/reserve`) vs new command stack (`ReserveCartStockCommand`, `ConsumeCartStockReservationsCommand`, `ReleaseCartStockReservationsCommand`) must converge onto one canonical reservation path
- **REQ-005**: Payment status for an order must be queryable over HTTP (currently `GetPaymentForCheckoutQuery` is in-process only; SPA has no polling endpoint)
- **REQ-006**: Storefront must support browsing products by taxon permalink: `GET /api/storefront/taxons/{permalink}` and `GET /api/storefront/taxons/{permalink}/products`
- **REQ-007**: Card-save flow (`SetupIntent`) must offer an HTTP attach/persist endpoint so saved PaymentMethods survive a session
- **SEC-001**: Stock reservations must remain cart-scoped (release requires `X-Cart-Token` or authenticated cart ownership); no endpoint may release another user's reservation
- **SEC-002**: Payment status endpoint must scope by `OrderId` and authenticate; no cross-user payment disclosure
- **CON-001**: Every C# feature follows `static partial class` vertical-slice layout: `Features/{Storefront}/{Feature}/{Action}/` with Handler, Endpoint, Request, Response, Validator (read-only queries may omit Request/Validator)
- **CON-002**: `TreatWarningsAsErrors=true` — any warning fails `dotnet build`
- **CON-003**: No Change in phase 5 (Identity, route base, cross-module refs) — out of scope
- **CON-004**: Module assembly is single; cross-module communication via MediatR `ISender` only — no new `using Module.X` beyond pre-existing contract usage
- **GUD-001**: Route constants live in `{Module}Feature.Storefront.cs` under `Features/Shared/`; new endpoints must add constants, not inline strings
- **GUD-002**: TTL single source: `InventoryFeature.Storefront.CartReservations.TTL_MINUTES = 30` consumed by old Request and new Command
- **PAT-001**: Follow existing crater patterns: `Result<T>`/`Result` returns, `.ToResult()` in endpoints, `ICommandHandler`/`PagedQueryHandler`, Carter `ICarterModule` for endpoint registration
- **PAT-002**: New endpoints mirror the closest existing sibling (e.g. `GetStoreTaxons` for taxon search, `CreatePaymentIntent` for payment status) — copy file-set + mapping style

## 2. Implementation Steps

### Implementation Phase 1: Fix Defects

- **GOAL-001**: Eliminate duplicate inventory DELETE route, converge reservation TTL to 30, align VisualSearchModels response contract

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/ReleaseSingle/` (`ReleaseSingleReservation.Endpoint.cs`, `ReleaseSingleReservation.cs`, `ReleaseSingleReservation.Command.cs`); keep `ReleaseCartReservation` (token-scoped). re-run `scripts/check-feature-conventions.sh` to confirm no dangling features | | |
| TASK-002 | Add `public const int TtlMinutesDefault = 30;` inside `InventoryFeature.Storefront.CartReservations` in `InventoryFeature.Storefront.cs` | | |
| TASK-003 | In `CartReservations/Reserve/ReserveCartStock.Request.cs:10` change `TtlMinutes = 15` → `= InventoryFeature.TtlMinutesDefault`; verify Validator accepts 30 range | | |
| TASK-004 | In `ReserveCartStock.Command.cs` set `TtlMinutes = 30` using same constant; in `AddToCart.cs:69` replace literal `30` with constant (import `InventoryFeature`) | | |
| TASK-005 | In `Products/VisualSearchModels/ListVisualSearchModels.Endpoint.cs` remove `.Produces(StatusCodes.Status400BadRequest)` (handler emits `Result<Response>` with no 400 path; keep only success + error docs that are real-reachable) | | |

### Implementation Phase 2: Consolidate Inventory Reservation Edge

- **GOAL-002**: Canonicalize cart-stock reservation: new command stack becomes single source of truth; legacy REST re-pointed to same TTL and semantics; availability consolidated

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Grep-verify all senders of `ReserveCartStockCommand`/`ConsumeCartStockReservationsCommand`/`ReleaseCartStockReservationsCommand` (`AddToCart.cs:71`, `CreatePaymentIntent.cs:50,112,126`, `CreateOrderFromCart.cs:68`) and all REST `cart/reserve` callers (`app/Store/src/features/inventory/services/cartReservationApi.ts`, `app/legacy/Storefront`) | | |
| TASK-007 | Confirm old REST `CartReservations/Reserve` (`POST /cart/reserve` line-level, `X-Cart-Token`) is still only reachable from live `app/Store` SPA — if CP finds no live caller, mark REST set deprecated (keep route, add deprecated doc-comment) instead of delete (respect user "keep ReleaseCartReservation" + SPA tests) | | |
| TASK-008 | Ensure new `ReserveCartStockCommand` covers listed usage old REST semantics need (multi-location `CountOnHand>0` picking vs per-line single location) — no behavior regression in `CreatePaymentIntent` flow; run Ordering unit tests touching reservation | | |
| TASK-009 | `scripts/check-feature-conventions.sh` + `dotnet build` green | | |

### Implementation Phase 3: Payment completion

- **GOAL-003**: Expose payment status over HTTP; add card-save attach endpoint passthrough

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Add `GET /api/storefront/payment/status/{orderId:guid}` — route constant in `PaymentFeature.Storefront.cs`, new feature dir `Payment/Features/Storefront/Payment/Status` with PagedQuery/Handler/Endpoint; wraps existing `GetPaymentForCheckoutQuery`; `RequireAuthorization`; returns `PaymentForCheckoutResponse` | | |
| TASK-011 | Add `POST /api/storefront/payment/methods/save` — Request carries `PaymentMethodId (Guid)` + optional `SetupIntentId/ReturnUrl`; Handler validates active non-deleted PaymentMethod via registry, then returns stored reference; route constant `PaymentMethods.Save` | | |
| TASK-012 | Re-run payment + checkout flow accuracy test: `CreateOrderFromCart` continues to call `GetPaymentForCheckoutQuery` (unchanged); new endpoint is read/reference wrapper only | | |

### Implementation Phase 4: Catalog taxon browsing

- **GOAL-004**: Add storefront browse-by-taxon-permalink endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | In `CatalogFeature.Storefront.cs` add constants: `Taxons` → `.ByPermalink.Route = "api/storefront/taxons/{permalink:regex(^[a-z0-9]+(?:-[a-z0-9]+)*$)}"` ready and on `48`; `Taxons` → `.Products` on path `"api/storefront/taxons/{permalink}/products"` | | |
| TASK-014 | New feature files `Features/Storefront/Products/List/GetByTaxon.cs` — replicate `GetStoreTaxons` handler shape (flat taxon list, filter by `taxonomyId`?) + `GetStoreTaxon.Products` reuses existing product-list query augmented by taxon overlap | | |
| TASK-015 | `GetTaxonsByPermalink` returns single taxon + breadcrumb chain; `GetProgramProduct by permalink` returns paged products bearing that taxon. map via `Shared/Mappings` | | |
| TASK-016 | `scripts/run-feature-conventions.sh`, `dotnet build`, `dotnet test service/Api/tests/Module.Test` green | task |

## 3. Alternatives

- **ALT-001**: Unify all bases to `api/storefront` (Identity/Location/Profile currently `api/store`) — rejected: large blast radius across 3 modules + SPA constant file + ApiTests; deferred to Phase 5 out of scope
- **ALT-002**: Delete legacy REST reservation API entirely — rejected: `ReleaseCartReservation` kept by user decision; risk of breaking `app/Store` spec mocks; deprecation + TTL convergence chosen instead
- **ALT-003**: Wait new payment endpoint type (webhook poll push) instead of HTTP pull-status — rejected: SPA needs simple polling; existing `GetPaymentForCheckoutQuery` reused

## 4. Dependencies

- **DEP-001**: `INotificationService` `Sinch`/SMS template, IF used by Identity (not used in Phases 1–4)
- **DEP-002**: Existing `PaymentCapture` table w `ResponseCode`=payment intent id mapping (`MarkPaymentPaid.cs` lookup) — status endpoint relies on same match
- **DEP-003**: `Catalog.Taxon` domain must already expose permalink/slug property (checked in Phase 4 header — confirm in `Taxon.Constant.cs`)
- **DEP-004**: For `List` depot: product query already nullable `TaxonCategory` filter path in `GetStorefrontProducts`; new permalink browse reuses it (verify during TASK-0015)

## 5. Files

- **FILE-001**: `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs` — TTL constant
- **FILE-002**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/ReleaseSingle/` — delete 3 files
- **FILE-003**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Request.cs` — TTL default
- **FILE-004**: `service/Api/src/Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.Command.cs` — TTL init
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs:69` — use TTL const
- **FILE-006**: `service/Api/src/Module/Catalog/Features/Storefront/Products/VisualSearchModels/ListVisualSearchModels.Endpoint.cs` — drop 400 `Produces`
- **FILE-007**: `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Storefront.cs` — new `PaymentMethods.Save`, `Payment.Status` routes
- **FILE-008**: `service/Api/src/Module/Payment/Features/Storefront/Payment/Status/*` — new dir
- **FILE-009**: `service/Api/src/Module/Payment/Features/Storefront/PaymentMethods/Save/*` — new dir
- **FILE-010**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Store.cs` — taxon permalink routes
- **FILE-011**: `service/Api/src/Module/Catalog/Features/Storefront/Products/List/GetByTaxon/*` — new dir handler/endpoint
- **FILE-012**: `service/Api/src/Module/Catalog/Features/Storefront/Classifications/Taxons/` — permalink single endpoint

## 6. Testing

- **TEST-001**: Ordering unit — `AddToCart` + `UpdateCartItemQuantity` reservation semantics unchanged (TTL 30)
- **TEST-002**: Inventory unit — `ReserveCartStockCommand` multi-location path (existing tests)
- **TEST-003**: Payment — new `Payment.Status` handler wraps `GetPaymentForCheckoutQuery` equal output; authenticated owner check
- **TEST-004**: Payment — `PaymentMethods/Save` rejects inactive/unknown method
- **TEST-005**: Catalog In directory — `GetStoreTaxons.Contribution` by taxonomy/permalink returns paged products; invalid Permalink → NotFound
- **TEST-006**: `scripts/check-feature-conventions.sh` — no orphan features from deletions
- **TEST-007**: `dotnet build` — zero warnings (warnings-as-errors)
- **TEST-008**: `cd app/Store && pnpm run lint && pnpm run test:unit` — SPA unchanged

## 7. Risks & Assumptions

- **RISK-001**: TTL change 15→30 lengthens hold; oversell window widened under high-competition stock — mitigated by serializable transaction + per-sum checks
- **RISK-002**: New `PaymentMethods.Save` returns may widen API surface with no persistence model if gateway stores only metadata — assumption: exist PaymentMethod table + validation
- **RISK-003**: Taxon permalink property may not exist yet in `Taxon` domain — verification needed during Phase 4 before coding (`Taxon.Constant.cs`)
- **RISK-004**: Deleting `ReleaseSingleReservation` removes the only un-token-single release; verify legacy `app/legacy/Storefront` does not hard-call it
- **ASSUMPTION-001**: No database migration needed for Phases 1–4 (all changes are code/route/TTL; no new EF entities)
- **ASSUMPTION-002**: `app/Store` current SPA does not directly call old REST `POST api/storefront/cart/reserve` (reservation flows through Ordering `AddToCart`) — confirm in TASK-006
- **ASSUMPTION-003**: Phase 5 (Identity + route base + cross-module) intentionally deferred per requisition

## 8. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — vertical slice & cross-module rules
- `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs`
- `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Storefront.cs`
- `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Store.cs`
- `AGENTS.md` — verification commands