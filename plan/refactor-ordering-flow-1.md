---
goal: Fix 8 ordering flow quality gaps across the Ordering backend and Store SPA checkout — runtime response-type bugs, missing line items, duplicate background service, dead code, and a checkout step race.
version: 1.0
date_created: 2026-08-12
last_updated: 2026-08-12
owner: Ordering team
status: 'In progress'
tags: [refactor, ordering, checkout, bug, store]
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

A review of the end-to-end ordering flow (Store SPA → API → back) found 10 issues remaining after the Storefront API Alignment (2025-08-11) was implemented. Eight require code changes; two (payment route under cart namespace, Payment state transition) are confirmed by-design and need no code. This plan implements the eight fixes: two backend handlers return full cart responses (fixing a Zod parse crash in the frontend), the order-detail response gains line items, the duplicate cart-expiry BackgroundService is removed, a stale TODO is cleaned, and the frontend checkout step-race and dead validate call are corrected.

Note: this plan explicitly permits cross-module reference files as needed. The cross-module reference drift check (`scripts/check-cross-module-refs.sh`) is **not** run as a gate for this work.

## 1. Requirements & Constraints

- **REQ-001**: `PATCH /api/storefront/cart/items/{lineItemId}` (UpdateCartItemQuantity) must return the full cart (`Result<CartDetailResponse>`), matching the existing AddItem behavior.
- **REQ-002**: `DELETE /api/storefront/cart/items/{lineItemId}` (RemoveCartItem) must return the full cart (`Result<CartDetailResponse>`).
- **REQ-003**: `GET /api/storefront/orders/{id}` response must include a populated `LineItems` collection so the Store SPA `OrderDetailView` renders rows instead of an empty table.
- **REQ-004**: Only one cart-expiry mechanism may run. The Hangfire `CartExpiryJobScheduler` stays; the `CartExpiryService` BackgroundService and its Loggers file are deleted and its DI registration removed.
- **REQ-005**: The `DeliveryRequired()` TODO comment must be removed; behavior stays `=> true`.
- **REQ-006**: The `useCheckout` composable must stop mutating `currentStep` in `createPaymentIntent()`; the view is the sole step-transition controller.
- **REQ-007**: The Store SPA Review step must re-fetch the cart and call the checkout-validate endpoint before advancing to step 4.
- **SEC-001**: No secrets, card data, or PII may be logged or added to any response DTO.
- **CON-001**: Warnings-as-errors (`TreatWarningsAsErrors=true`) — every backend change must build with zero warnings.
- **CON-002**: Result objects, not exceptions — backend handlers return `Result<T>` / `Result`.
- **CON-003**: Vertical slice feature files — each feature stays a `static partial class` split across Handler/Request/Response/Endpoint/Validator files.
- **CON-004**: Cross-module namespace references are permitted as needed for this plan; `scripts/check-cross-module-refs.sh` is not a gate.
- **CON-005**: Store SPA comments follow the Store AGENTS.md standard (`// Label: Sentence.` in script; `<!-- Section: Title — purpose -->` in template).
- **CON-006**: Store SPA lint and unit tests must pass with zero warnings (`pnpm run lint` + `pnpm run test:unit`).
- **GUD-001**: Mirror the existing `AddToCart` handler's cart-item enrichment pattern (`BuildCartItemLookupAsync` + `MapToDetailWithItems<T>`) in the update/remove handlers rather than inventing a new mapping style.
- **GUD-002**: Reuse the existing `LineItemResponse` DTO and `MapToLineItemResponse<T>()` mapping for order line items; do not create new response types.
- **PAT-001**: Response records inherit `CartDetailResponse` (same as `AddToCart.Response`).
- **PAT-002**: Endpoint `.Produces<Result>()` signatures are widened to `.Produces<Result<Response>>()` in lockstep with the handler return-type change.

## 2. Implementation Steps

### Implementation Phase 1: Backend cart mutation returns full cart

- GOAL-001: Make the update-item and remove-item endpoints return the enriched full cart, eliminating the frontend Zod parse crash.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Response.cs` (`Response : CartDetailResponse`). Rework `UpdateCartItemQuantity.cs`: command → `ICommand<Response>`, handler → `ICommandHandler<Command, Response>`, `Handle` returns `Task<Result<Response>>`, add Catalog + CartMapping usings, return `cart.MapToDetailWithItems<Response>(itemLookup)` after `SaveChangesAsync`, and add the private `BuildCartItemLookupAsync` helper (copied from `AddToCart.cs`). Update `UpdateCartItemQuantity.Endpoint.cs` `.Produces<Result>()` → `.Produces<Result<Response>>()`. | |  |
| TASK-002 | Create `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Response.cs` (`Response : CartDetailResponse`). Rework `RemoveCartItem.cs`: command → `ICommand<Response>`, handler → `ICommandHandler<Command, Response>`, `Handle` returns `Task<Result<Response>>`, add Catalog + CartMapping usings, replace the final `Result.Ok(...)` with a full-cart return, and add the private `BuildCartItemLookupAsync` helper. Update `RemoveCartItem.Endpoint.cs` `.Produces<Result>()` → `.Produces<Result<Response>>()`. | |  |

### Implementation Phase 2: Order detail line items

- GOAL-002: Expose order line items in the storefront order-detail response so the frontend can render them.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Add `public List<LineItemResponse> LineItems { get; init; } = [];` to `OrderDetailResponse` in `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs`. In `MapToDetail<T>()` in `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs`, add `LineItems = entity.LineItems.Select(li => li.MapToLineItemResponse<LineItemResponse>()).ToList(),` after `ModifiedAtUtc`. | |  |

### Implementation Phase 3: Remove duplicate background service + dead code

- GOAL-003: Reduce cart expiry to Hangfire only and clean the stale `DeliveryRequired()` TODO.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Delete `service/Api/src/Module/Ordering/Services/CartExpiryService.cs` and `service/Api/src/Module/Ordering/Services/CartExpiryService.Loggers.cs`. In `service/Api/src/Module/Ordering/Ordering.Extension.cs` remove the comment block and `builder.Services.AddHostedService<Services.CartExpiryService>();`; keep `AddScoped<Backgrounds.CartExpiryJob>()` and `AddHostedService<Backgrounds.CartExpiryJobScheduler>()`. | |  |
| TASK-005 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` remove the two `// TODO: Implement product-type-based delivery check...` comment lines above `public static bool DeliveryRequired() => true;`. | |  |

### Implementation Phase 4: Frontend checkout step race + Review validation

- GOAL-004: Make the view the sole step controller and wire cart re-fetch + checkout validation into the Review step.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | In `app/Store/src/features/ordering/composables/useCheckout.ts`: delete `currentStep.value = 4` inside `createPaymentIntent()`; add an `async validateCheckout(): Promise<boolean>` method (wraps `CheckoutApi.validateCheckout()`, sets `error` on failure, returns success bool); export `validateCheckout` in the returned reactive object. | |  |
| TASK-007 | In `app/Store/src/features/ordering/views/CheckoutView.vue` rework `advanceToReview()`: guard `!checkout.paymentClientSecret`; set `checkout.loading`/`checkout.error`; `await cart.fetchCart()`; if OK `await checkout.validateCheckout()`; on both success set `checkout.currentStep = 4`. | |  |

### Implementation Phase 5: Verification

- GOAL-005: Prove all changes build clean and pass existing tests.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Run `dotnet build` (zero warnings), `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`, and `cd app/Store && pnpm run lint && pnpm run test:unit`. Run `bash scripts/check-feature-conventions.sh` (feature completeness). Do **not** run `scripts/check-cross-module-refs.sh`. Fix any failures before commit. | |  |

## 3. Alternatives

- **ALT-001**: Frontend refetch-after-void (`Result<void>` + separate `getCart()`) for update/remove — rejected: doubles API calls per cart mutation and leaves the backend response contract inconsistent with AddItem. Chose backend full-cart returns.
- **ALT-002**: Add `CheckoutState.Review` enum value and an `advance-review` endpoint so Review is a backend state — rejected during brainstorming: adds state-machine complexity and an extra API call for a pure UI pause. Chose UI-only Review with cart re-fetch.
- **ALT-003**: Keep both cart-expiry mechanisms (BackgroundService + Hangfire) — rejected: redundant hourly processing with no fault-tolerance benefit. Chose Hangfire only (retry + dashboard visibility).

## 4. Dependencies

- **DEP-001**: `Module.Catalog.Domain.Variants` / `Module.Catalog.Domain.Products` — variant/product lookup used by `BuildCartItemLookupAsync` in the update/remove handlers (cross-module reference explicitly permitted).
- **DEP-002**: Existing `CartMapping.MapToDetailWithItems<T>()`, `CartItemLookup`, and `CartDetailResponse` — the enrichment/mapping base for TASK-001 and TASK-002.
- **DEP-003**: Existing `LineItemResponse` and `OrderMapping.MapToLineItemResponse<T>()` — reused by TASK-003.
- **DEP-004**: `CheckoutApi.validateCheckout()` (already in the Store SPA API client) — consumed by TASK-006/TASK-007.
- **DEP-005**: `cart.fetchCart()` on the shared `useCart` singleton — consumed by TASK-007.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Response.cs` — create.
- **FILE-002**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` — modify.
- **FILE-003**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Endpoint.cs` — modify.
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Response.cs` — create.
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs` — modify.
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs` — modify.
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` — modify.
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs` — modify.
- **FILE-009**: `service/Api/src/Module/Ordering/Services/CartExpiryService.cs` — delete.
- **FILE-010**: `service/Api/src/Module/Ordering/Services/CartExpiryService.Loggers.cs` — delete.
- **FILE-011**: `service/Api/src/Module/Ordering/Ordering.Extension.cs` — modify.
- **FILE-012**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` — modify.
- **FILE-013**: `app/Store/src/features/ordering/composables/useCheckout.ts` — modify.
- **FILE-014**: `app/Store/src/features/ordering/views/CheckoutView.vue` — modify.

## 6. Testing

- **TEST-001**: Backend build — `dotnet build` passes with zero warnings (warnings-as-errors gate).
- **TEST-002**: Ordering unit tests — `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"` all pass; update any existing `MapToDetail`/response-shape assertions that now include `LineItems`.
- **TEST-003**: Cart expiry unit tests — `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CartExpiry"` all pass after removing `CartExpiryService`.
- **TEST-004**: Store SPA — `pnpm run lint` and `pnpm run test:unit` pass (touches `useCheckout.ts`, `CheckoutView.vue`).
- **TEST-005**: Feature conventions — `bash scripts/check-feature-conventions.sh` passes (no feature file drift).
- **TEST-006**: Manual smoke — `ApiTests/Ordering/demo-flow.http` full checkout flow still succeeds end-to-end (requires running API; optional if integration environment unavailable).

## 7. Risks & Assumptions

- **RISK-001**: Widening response types could break existing unit tests that assert exact `OrderDetailResponse`/`Result` shapes — mitigation: update affected assertions (TEST-002).
- **RISK-002**: Duplicating `BuildCartItemLookupAsync` across three handlers (AddItem, UpdateItem, RemoveItem) increases maintenance surface — accepted for scope; a shared helper extraction is a follow-up, not part of this plan.
- **ASSUMPTION-001**: `LineItemResponse` fields (`VariantId`, `Quantity`, `Price`, `Total`, `Currency`, `CreatedAtUtc`) are sufficient for the Store SPA order-detail line-items table; catalog enrichment (name/image) is deferred.
- **ASSUMPTION-002**: Cross-module references added by TASK-001/TASK-002 are acceptable; `scripts/check-cross-module-refs.sh` is not a gate for this plan.
- **ASSUMPTION-003**: Hangfire is configured and running in the deployment target, so the BackgroundService removal is safe.

## 8. Related Specifications / Further Reading

- [Ordering Flow Fixes Design Spec](../docs/superpowers/specs/2026-08-12-ordering-flow-fixes-design.md)
- [Storefront API Alignment Design (2025-08-11)](../docs/superpowers/specs/2025-08-11-storefront-api-alignment-design.md)
