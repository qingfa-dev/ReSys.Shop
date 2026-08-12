# Ordering Flow Fixes Design

**Status:** Draft
**Date:** 2026-08-12
**Reference:** Storefront API Alignment Design (2025-08-11)
**Affected Modules:** Ordering (backend), Store SPA (frontend)

---

## 1. Context

A review of the end-to-end ordering flow (Store SPA → API → back) found 10 issues
remaining after the Storefront API Alignment (2025-08-11) was implemented. These
range from runtime bugs (response type mismatches that throw on every cart
mutation) to design gaps (duplicate background services, dead code, stale
comments).

The 2025 alignment spec already fixed route reorganization, HTTP method
corrections, and added inventory endpoints. This spec targets the remaining
quality gaps that actively degrade the checkout experience.

---

## 2. Issues and Fixes

### 2.1 Runtime Bugs

**Issue 1-2: Response type mismatches on `updateItem` and `removeItem`**

- `CartApi.updateItem()` and `CartApi.removeItem()` expect `Result<CartResponse>`
  and call `CartResponseSchema.parse(result.value)`
- Backend `UpdateCartItemQuantity.Endpoint.cs` and `RemoveCartItem.Endpoint.cs`
  produce `Produces<Result>()` — untyped, no value
- `result.value` is `undefined`, Zod validation throws at runtime

**Fix:** Change both command handlers to return `Result<Response>` where
`Response` is the full cart DTO. After the mutation, reload the cart from the
database and map to the response. Update endpoint `Produces<>` signatures to
match. Frontend stays unchanged — it already expects `CartResponse` and parses
with Zod.

**Issue 3: Missing line items in storefront order detail**

- `GetCustomerOrder` handler does `.Include(x => x.LineItems)` eagerly
- `MapToDetail<T>()` maps all order fields but excludes line items
- `OrderDetailResponse` DTO has `ItemCount` and `ItemTotal` but no `LineItems`
  collection
- Frontend `OrderDetailView.vue` renders an empty table with a tooltip:
  "Line items are not exposed by the order API yet"

**Fix:** Add `List<LineItemResponse> LineItems` to `OrderDetailResponse`. Update
`MapToDetail<T>()` to project `.LineItems.Select(li =>
li.MapToLineItemResponse<LineItemResponse>()).ToList()`. The frontend
already reads from a line items array — once populated, it renders immediately.

No catalog enrichment (product name, image) at this stage — LineItemResponse
carries `VariantId`, `Quantity`, `Price`, `Total`, `Currency`, `CreatedAtUtc`
which is sufficient for the order detail display.

### 2.2 Design Gaps

**Issue 4: Frontend "Review" step has no backend `CheckoutState` analog**

Backend state machine: `Address → Delivery → Payment → Confirm → Complete`
Frontend wizard: `Shipping → Delivery → Payment → Review → Confirmation`

The Review step is pure UI — no API call. When the user clicks "Place Order" in
the Review panel, `POST /cart/checkout` requires `CheckoutState.Payment` and the
backend advances to `Confirm → Complete` atomically.

**Fix:** Review remains UI-only. No changes to `CheckoutState` enum. The Review
step re-fetches the cart (`GET /cart`) and validates readiness (`GET
/cart/checkout`) before displaying the summary. This guarantees the user sees
current totals, shipping costs, and line items before committing.

**Issue 5: `currentStep` race between composable and view**

`useCheckout.createPaymentIntent()` line 92 sets `currentStep = 4` on success.
Simultaneously, `CheckoutView.vue` watcher (lines 197-213) observes `step === 3`
and resets `checkout.currentStep = 3` to hold the user on the payment panel.
This works by accident — the watcher re-fires on the 4→3 change and finds
`paymentClientSecret` already set, so it skips repeat intent creation.

**Fix:** Remove `currentStep.value = 4` from `useCheckout.ts:92`. The composable
mutates data only; step transitions are exclusively controlled by the view's
button handlers (`advanceToReview()`, etc.).

**Issue 7: Dual cart expiry mechanisms**

Both `CartExpiryService` (BackgroundService, 1-hour `Task.Delay` loop) and
`CartExpiryJobScheduler` (Hangfire, hourly cron `0 * * * *`) run the same expiry
logic. They process the same query (`Draft orders older than 7 days`) and are
idempotent, but doubling the processing is wasteful.

**Fix:** Remove `CartExpiryService` (BackgroundService + Loggers files). Remove
its registration from `Ordering.Extension.cs`. Keep Hangfire
`CartExpiryJobScheduler` only — it provides retry, dashboard visibility, and
cron scheduling.

**Issue 8: `DeliveryRequired()` hardcoded to `true`**

`Order.Method.Checkout.cs:70` returns `true` unconditionally with a TODO comment
about checking product type (physical vs digital). Catalog domain has no
product type concept, so implementing the check requires cross-module schema
changes.

**Fix:** Remove the TODO comment. Keep returning `true`. The MVP only handles
physical products. Add digital product support as a future Catalog feature.

**Issue 10: Dead `validateCheckout` API call**

`CheckoutApi.validateCheckout()` exists in the frontend API client but is never
called by any component or composable. The backend `GET /cart/checkout`
endpoint validates address, items, shipping method, and email before checkout.

**Fix:** Wire `validateCheckout()` into `advanceToReview()` in `CheckoutView.vue`.
The Review step calls it alongside the cart re-fetch to confirm backend
readiness before showing the summary.

### 2.3 By-Design (No Change)

**Issue 6:** `/api/storefront/cart/payment/intent` is a Billing module endpoint
under the Cart URL namespace. This is intentional per the 2025 alignment spec
(section 4.1: "Payment lives under cart — payment session is a cart
sub-resource"). No change needed.

**Issue 9:** The Billing `CreatePaymentIntent` handler calls
`AdvanceCheckoutState(Payment)` via MediatR (confirmed:
`Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:139`).
The `Payment` state transition is already correct. No change needed.

---

## 3. File Change Map

### Backend — Ordering Module

| # | File | Change |
|---|------|--------|
| 1 | `Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` | Handler returns `Result<Response>`, reload cart + map |
| 1 | `Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Endpoint.cs` | `Produces<Result<Response>>` |
| 2 | `Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs` | Handler returns `Result<Response>`, reload cart + map |
| 2 | `Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs` | `Produces<Result<Response>>` |
| 3 | `Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` | Add `List<LineItemResponse> LineItems` to `OrderDetailResponse` |
| 3 | `Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs` | `MapToDetail<T>()` maps `.LineItems` to response |
| 7 | `Backgrounds/CartExpiryService.cs` | **Delete** |
| 7 | `Backgrounds/CartExpiryService.Loggers.cs` | **Delete** |
| 7 | `Ordering.Extension.cs` | Remove `CartExpiryService` registration |
| 8 | `Domain/Orders/Order.Method.Checkout.cs` | Remove TODO comment on `DeliveryRequired()` |
| 1-2 | `Features/Storefront/Cart/Shared/Models/Cart.Model.Response.cs` | Add `Response` type or reuse existing `CartDetailResponse` for updateItem/removeItem handlers |

### Frontend — Store SPA

| # | File | Change |
|---|------|--------|
| 5 | `features/ordering/composables/useCheckout.ts:92` | Remove `currentStep.value = 4` from `createPaymentIntent()` |
| 4+10 | `features/ordering/composables/useCheckout.ts` | Add `validateCheckout()` method wrapping `CheckoutApi.validateCheckout()` |
| 4+10 | `features/ordering/composables/useCheckout.ts` | Add `refetchCart` callback parameter or expose `fetchCart` |
| 4+10 | `features/ordering/views/CheckoutView.vue:231-233` | `advanceToReview()` calls `cart.fetchCart()` + `checkout.validateCheckout()` before `currentStep = 4` |

---

## 4. Impact Assessment

| Area | Risk | Notes |
|------|:----:|-------|
| Backend breaking changes | None | All changes are additive (new fields in response DTOs, wider return types) |
| Frontend breaking changes | None | Response parsing unchanged; composable removes a line, view adds calls |
| Database schema | None | No migrations |
| API contracts | Low | Response shapes expand — existing consumers unaffected |
| Cross-module isolation | None | No new cross-module references introduced |

---

## 5. Verification

- `dotnet build` — warnings-as-errors, must pass
- `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"` — unit tests
- `dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~Ordering"` — integration tests (requires Docker)
- `bash scripts/check-feature-conventions.sh` — feature file completeness
- `cd app/Store && pnpm run lint && pnpm run test:unit` — frontend verification
- Manual: run `ApiTests/Ordering/demo-flow.http` smoke test — full checkout flow

---

## 6. Success Criteria

1. `CartApi.updateItem()` and `CartApi.removeItem()` return full cart data
2. `GET /orders/{id}` response includes line items (at minimum: `VariantId`, `Quantity`, `Price`, `Total`)
3. Review step re-fetches cart and validates checkout before showing summary
4. No `currentStep` race — composable never sets step, view controls all transitions
5. Only one cart expiry mechanism active (Hangfire)
6. No stale TODO on `DeliveryRequired()`
7. `validateCheckout` is called during the Review flow
8. All existing tests pass
