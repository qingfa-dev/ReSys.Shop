---
goal: Fix 8 Ordering Flow Quality Gaps — Cart Mutation Responses, Order Line Items, Background Cleanup, Checkout Wizard
version: 1.0
date_created: 2026-08-12
last_updated: 2026-08-12
owner: ReSys.Shop
status: 'In progress'
tags: [ordering, storefront, bug, refactor, checkout]
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

Fix 8 quality gaps in the Ordering backend and Store SPA checkout flow identified by an end-to-end flow review (Store SPA → API → back). Two runtime bugs throw on every cart mutation (response type mismatch), the order detail response omits line items, duplicate cart-expiry services run concurrently, and the frontend checkout wizard has a step-race and a dead validation call. All fixes are additive — no schema changes, no HTTP contract breaks.

## 1. Requirements & Constraints

- **REQ-001**: .NET 10, `TreatWarningsAsErrors=true` — `dotnet build` must be 0 errors / 0 warnings after every task
- **REQ-002**: Store SPA `pnpm run lint` and `pnpm run test:unit` must pass after frontend tasks
- **REQ-003**: `CartApi.updateItem()` and `CartApi.removeItem()` must receive a full `CartDetailResponse` (fixing the Zod `CartResponseSchema.parse(undefined)` crash)
- **REQ-004**: `GET /api/storefront/orders/{id}` response must include `LineItems` (VariantId, Quantity, Price, Total, Currency, CreatedAtUtc)
- **REQ-005**: Only one cart-expiry mechanism may remain active (Hangfire `CartExpiryJobScheduler`)
- **REQ-006**: `useCheckout.createPaymentIntent()` must not set `currentStep` — the view controls all step transitions
- **REQ-007**: The Review step must call `cart.fetchCart()` and `CheckoutApi.validateCheckout()` before advancing to step 4
- **SEC-001**: No new cross-module namespace references beyond the accepted Ordering→Catalog enrichment refs (CartMapping shared helper + UpdateItemQuantity/RemoveItem handlers, per human decision 2026-08-12)
- **CON-001**: No database schema changes, no EF Core migrations
- **CON-002**: All changes are additive — existing HTTP consumers remain unaffected
- **CON-003**: Do NOT change backend `CheckoutState` enum (Review stays UI-only)
- **GUD-001**: Follow the existing `AddToCart` handler pattern for cart-mutation handlers — enrichment via shared `CartMapping.BuildCartItemLookupAsync` + `MapToDetailWithItems<Response>` (extracted to one shared helper, not duplicated)
- **GUD-002**: Vertical slice feature files — `static partial class` split across Handler/Request/Response/Endpoint/Validator
- **PAT-001**: Feature response records inherit `CartDetailResponse` (see `AddToCart.Response.cs`)
- **PAT-002**: Registration removals use the existing `Ordering.Extension.cs` boundary — no domain logic in DI registration

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Make cart-mutation endpoints return a full cart response so the Store SPA Zod parsers stop crashing on `undefined`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Extract `BuildCartItemLookupAsync` into the shared `CartMapping` partial class (`Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs`), refactor `AddToCart.cs` and `GetCart.cs` to call the shared version and delete their private copies. Add `UpdateCartItemQuantity.Response : CartDetailResponse`; change handler to `ICommandHandler<Command, Response>` returning `Task<Result<Response>>`; after save, call shared `CartMapping.BuildCartItemLookupAsync` and return `cart.MapToDetailWithItems<Response>(itemLookup)`; update endpoint `.Produces<Result<Response>>()`. Files: `Cart.Mapping.Model.cs`, `AddToCart.cs`, `GetCart.cs`, `UpdateCartItemQuantity.cs`, new `UpdateCartItemQuantity.Response.cs`, `UpdateCartItemQuantity.Endpoint.cs`. | ⬜ | |
| TASK-002 | Add `RemoveCartItem.Response : CartDetailResponse`; change handler to `ICommandHandler<Command, Response>` returning `Task<Result<Response>>`; after save, call shared `CartMapping.BuildCartItemLookupAsync` + `MapToDetailWithItems<Response>`; update endpoint `.Produces<Result<Response>>()`. Files: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs`, new `RemoveCartItem.Response.cs`, `RemoveCartItem.Endpoint.cs`. | ⬜ | |

### Implementation Phase 2

- GOAL-002: Expose order line items to the storefront and clean up two Ordering background/TODO gaps.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Add `List<LineItemResponse> LineItems { get; init; } = [];` to `OrderDetailResponse`; populate in `MapToDetail<T>()` via `entity.LineItems.Select(li => li.MapToLineItemResponse<LineItemResponse>()).ToList()`. Files: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs`, `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs`. | ⬜ | |
| TASK-004 | Delete `service/Api/src/Module/Ordering/Services/CartExpiryService.cs` and `CartExpiryService.Loggers.cs`; remove `builder.Services.AddHostedService<Services.CartExpiryService>();` and its comment block from `Ordering.Extension.cs` (keep `CartExpiryJob` scoped + `CartExpiryJobScheduler` hosted). | ⬜ | |
| TASK-005 | Remove the 2-line TODO comment above `DeliveryRequired()` in `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` (keep `public static bool DeliveryRequired() => true;`). | ⬜ | |

### Implementation Phase 3

- GOAL-003: Fix the checkout wizard step race and wire validation into the Review step.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | In `app/Store/src/features/ordering/composables/useCheckout.ts`: delete `currentStep.value = 4` inside `createPaymentIntent()`; add `async function validateCheckout(): Promise<boolean>` wrapping `CheckoutApi.validateCheckout()` (sets `error` on failure, guards `loading`); expose `validateCheckout` in the returned reactive object. | ⬜ | |
| TASK-007 | In `app/Store/src/features/ordering/views/CheckoutView.vue`: rewrite `advanceToReview()` so that when `checkout.paymentClientSecret` is set it sets `checkout.loading = true`, awaits `cart.fetchCart()`, awaits `checkout.validateCheckout()` (only if cart fetch succeeded), resets `checkout.loading = false`, and only sets `checkout.currentStep = 4` when both succeed. | ⬜ | |

### Implementation Phase 4

- GOAL-004: Verify the whole branch — build, unit tests, SPA checks, feature-convention drift, then commit.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Run `dotnet build` (0/0), `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`, `cd app/Store && pnpm run lint && pnpm run test:unit`, `bash scripts/check-feature-conventions.sh`. Fix any failures introduced by Tasks 1-7, then commit all changes as `fix(ordering): resolve 8 ordering flow quality gaps`. | ⬜ | |

## 3. Alternatives

- **ALT-001**: Change the frontend to expect `Result<void>` from updateItem/removeItem and call `getCart()` afterward. Rejected: adds a second HTTP round-trip per mutation and moves correctness burden to the SPA; the backend `AddToCart` handler already returns a full cart, so mirroring it is the consistent pattern.
- **ALT-002**: Add `CheckoutState.Review` to the backend enum and a dedicated `Payment → Review` transition endpoint. Rejected by user decision (CON-003): the Review panel is a UX concern; backend state remains `Address → Delivery → Payment → Confirm → Complete`.
- **ALT-003**: Embed the Review-step validation into the payment-intent creation response. Rejected: couples Billing's intent response to Ordering's checkout-wizard concerns; explicit `validateCheckout()` keeps concerns separate.

## 4. Dependencies

- **DEP-001**: Storefront API Alignment spec (2025-08-11) — cart routes and methods already aligned; this plan only fixes remaining response/state gaps.
- **DEP-002**: `CartMapping.MapToDetailWithItems<T>()` + `CartItemLookup` already exist in `Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs`; `BuildCartItemLookupAsync` is extracted into this same file (currently private in `AddToCart.cs` + `GetCart.cs`)
- **DEP-003**: `OrderMapping.MapToLineItemResponse<T>()` and `LineItemResponse` already exist in `Features/Admin/Orders/Shared/`.
- **DEP-004**: Hangfire `CartExpiryJobScheduler` + `CartExpiryJob` are registered and functional — removing the BackgroundService fallback is safe.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` — TASK-001 (extract shared `BuildCartItemLookupAsync`)
- **FILE-002**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — TASK-001 (use shared helper, delete private copy)
- **FILE-003**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.cs` — TASK-001 (use shared helper, delete private copy)
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` — TASK-001 (handler → `Result<Response>`)
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Response.cs` — TASK-001 (new)
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Endpoint.cs` — TASK-001 (Produces)
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs` — TASK-002 (handler → `Result<Response>`)
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Response.cs` — TASK-002 (new)
- **FILE-009**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs` — TASK-002 (Produces)
- **FILE-010**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` — TASK-003 (LineItems property)
- **FILE-011**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs` — TASK-003 (MapToDetail line items)
- **FILE-012**: `service/Api/src/Module/Ordering/Services/CartExpiryService.cs` — TASK-004 (delete)
- **FILE-013**: `service/Api/src/Module/Ordering/Services/CartExpiryService.Loggers.cs` — TASK-004 (delete)
- **FILE-014**: `service/Api/src/Module/Ordering/Ordering.Extension.cs` — TASK-004 (remove registration)
- **FILE-015**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` — TASK-005 (remove TODO)
- **FILE-016**: `app/Store/src/features/ordering/composables/useCheckout.ts` — TASK-006 (step race + validateCheckout)
- **FILE-017**: `app/Store/src/features/ordering/views/CheckoutView.vue` — TASK-007 (advanceToReview)
- **FILE-018**: `docs/superpowers/plans/2026-08-12-ordering-flow-fixes.md` — source plan (superseded by this file)

## 6. Testing

- **TEST-001**: `dotnet build` — 0 errors / 0 warnings after each backend task
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"` — all Ordering unit tests pass (existing `UpdateCartItemQuantity.Tests.cs`, `RemoveCartItem.Tests.cs`, `Cart.Mapping.Tests.cs`, `Order.Mapping.Tests.cs` must be updated where response shape changed)
- **TEST-003**: `cd app/Store && pnpm run lint && pnpm run test:unit` — passes after TASK-006/007
- **TEST-004**: `bash scripts/check-feature-conventions.sh` — no AC-001/002/003/005 drift
- **TEST-005**: Manual smoke — `ApiTests/Ordering/demo-flow.http` full checkout flow still succeeds (requires Aspire runtime)

## 7. Risks & Assumptions

- **RISK-001**: Existing Ordering unit tests assert on the old `Result` (untyped) return of update/remove handlers — they must be updated to assert `Result<Response>` with the full cart, which may touch several test files not listed in FILE-001..015.
- **RISK-002**: `MapToDetail<T>()` is shared by storefront and admin order-detail mappings — adding `LineItems` changes both response shapes; additive only, but integration tests asserting exact response bodies may need updating.
- **ASSUMPTION-001**: `CartItemLookup` enrichment (SKU, product name, image) in the update/remove responses is desirable and mirrors `AddToCart`; acceptable added query cost per mutation.
- **ASSUMPTION-002**: The frontend already parses `CartResponse` on update/remove success, so no `cartApi.ts` change is required — only backend response shape changes.

## 8. Related Specifications / Further Reading

- [Ordering Flow Fixes Design Spec](docs/superpowers/specs/2026-08-12-ordering-flow-fixes-design.md)
- [Storefront API Alignment Design](docs/superpowers/specs/2025-08-11-storefront-api-alignment-design.md)
- [Source Implementation Plan](docs/superpowers/plans/2026-08-12-ordering-flow-fixes.md)
