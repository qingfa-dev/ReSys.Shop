---
goal: Correct checkout state sync between the Ordering backend and the Store SPA — backend CheckoutState becomes the single source of truth, amount-affecting edits regress to Delivery, and the wizard allows re-doing a step (change address / change shipping method).
version: 1.0
date_created: 2026-08-12
last_updated: 2026-08-12
owner: Ordering team
status: 'Planned'
tags: [refactor, ordering, checkout, store, state-machine, bug]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The checkout wizard in the Store SPA drives its five panels from a purely local `currentStep` ref that is never hydrated from the backend `CheckoutState` (`useCheckout.ts:15`, `useCart.ts:24-29`), so a returning user whose cart already advanced (e.g. shipping method selected → backend `Delivery`) still lands on the Address panel. The backend also rejects re-selecting a shipping method because `SelectShippingRate` unconditionally calls `AdvanceCheckoutState(Delivery)` (`SelectShippingRate.cs:69`) while the transition whitelist is strictly forward-only (`Order.Method.Checkout.cs:98-113`), and no rule invalidates a payment intent whose amount becomes stale after an edit. This plan makes the backend the single source of truth, makes state advancement idempotent, adds a domain regression rule for amount-affecting edits, and re-wires the SPA to derive its step from the backend while allowing back/same-step navigation. Billing-module changes (old-intent voiding, intent re-creation at `Payment`, amount-mismatch guard) are explicitly deferred (CON-006).

## 1. Requirements & Constraints

- **REQ-001**: Backend `CheckoutState` (`Address → Delivery → Payment → Confirm → Complete`) is the single source of truth for the Store SPA checkout step; the SPA must never assume a step from local action alone.
- **REQ-002**: `Order.AdvanceCheckoutState(CheckoutState target)` must treat `target == CheckoutState` as a valid idempotent no-op so re-selecting a shipping method at `Delivery` does not fail with `InvalidCheckoutTransition`.
- **REQ-003**: Any Draft-order mutation that changes `Total` while `CheckoutState >= Payment` must regress `CheckoutState` to `Delivery` so payment is redone with the correct amount.
- **REQ-004**: `PATCH /api/storefront/cart/shipping-rate` must succeed when the cart is already at `Delivery` (change shipping method on the same step).
- **REQ-005**: `PATCH /api/storefront/cart` must keep allowing address/email changes at any Draft state and must advance `Address → Delivery` when both addresses become set, so the SPA step maps cleanly to backend state.
- **REQ-006**: `GET /api/storefront/cart` response must include `shippingMethodId`, `shipAddressId`, `email` so the SPA can prefill on re-entry/reload.
- **REQ-007**: Store SPA `useCart` must persist `checkoutState`, `shippingMethodId`, `shipAddressId`, `email` returned by `GET /api/storefront/cart`.
- **REQ-008**: Store SPA `useCheckout` must derive `backendStep` from the backend `checkoutState` and expose `displayStep` (the panel shown), clamped down only when the backend state regresses.
- **REQ-009**: Store SPA must re-fetch the cart (`cart.fetchCart(true)`) after every checkout mutation instead of writing optimistic backend step values.
- **REQ-010**: When the backend state regresses from `Payment` to `Delivery`, the SPA must clear `paymentClientSecret`, `paymentIntentId`, `paymentMethodId` so the payment intent is re-created.
- **SEC-001**: No secrets, card data, or PII may be logged or added to any response DTO.
- **CON-001**: Warnings-as-errors (`TreatWarningsAsErrors=true`) — every backend change must build with zero warnings.
- **CON-002**: Result objects, not exceptions — backend domain operations return `Result` / `Result<T>`.
- **CON-003**: Vertical slice feature files — each feature stays a `static partial class` split across Handler/Request/Response/Endpoint/Validator files; domain logic stays in `Domain/Orders/Order.Method.*.cs` partial files.
- **CON-004**: Store SPA comments follow the Store AGENTS.md standard (`// Label: Sentence.` in script; `<!-- Section: Title — purpose -->` in template).
- **CON-005**: Store SPA lint and unit tests must pass with zero warnings (`pnpm run lint` + `pnpm run test:unit`).
- **CON-006**: Billing module changes (`CreatePaymentIntent` guard, old-intent voiding, amount-mismatch guard at placement) are OUT of scope for this plan and deferred to a follow-up plan.
- **CON-007**: No `git stash`, `git restore`, `git revert`, `git checkout <ref> -- <path>`, or `git reset --hard` may be run during this work (AGENTS.md non-negotiable rule 6).
- **GUD-001**: Keep the state-machine logic in the existing `Order.Method.Checkout.cs` partial class using the established `Result`-returning method style; do not create new service abstractions.
- **GUD-002**: Reuse the existing re-fetch pattern (`cart.fetchCart(true)`) and the existing `CartResponseBase` / `MapToDetail<T>` mapping rather than adding new endpoints.
- **PAT-001**: Step mapping — backend state to SPA step index: `Address→1`, `Delivery→2`, `Payment→3`, `Confirm→4`, `Complete→5`; unknown/empty state maps to `1`.
- **PAT-002**: Regression guard pattern — capture `var previousTotal = cart.Total;` before the mutation, call `cart.RegressCheckoutIfAmountChanged(previousTotal)` after `RecalculateTotals()` (or equivalent) and before `SaveChangesAsync`.
- **PAT-003**: Idempotent advance pattern — add the same-state case to the existing `AdvanceCheckoutState` switch whitelist rather than branching at call sites.

## 2. Implementation Steps

### Implementation Phase 1: Backend domain state machine

- GOAL-001: Make `AdvanceCheckoutState` idempotent and add the amount-change regression rule to the `Order` domain.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`, modify `AdvanceCheckoutState(CheckoutState target)` (~lines 98-113): add a case `(CheckoutState target) when target == CheckoutState => true` to the `validTransition` switch so advancing to the current state is an idempotent no-op. Do not change any other transition. | |  |
| TASK-002 | In the same file, add `public Result RegressCheckoutIfAmountChanged(decimal previousTotal)` to the `Order` partial class: if `Status == OrderStatus.Draft && CheckoutState >= CheckoutState.Payment && Total != previousTotal`, set `CheckoutState = CheckoutState.Delivery`. Always return `Result.Ok()`. | |  |
| TASK-003 | In the same file, modify `SetShippingMethod(Guid methodId)` (~lines 210-222): capture `var previousTotal = Total;` as the first statement, and call `RegressCheckoutIfAmountChanged(previousTotal)` immediately after `RecalculateTotals()` and before returning `Result.Ok(...)`. | |  |

### Implementation Phase 2: Backend handler wiring

- GOAL-002: Wire the regression rule into every Draft-order handler that can change `Total`, and make `UpdateCheckout` advance to `Delivery` when both addresses are set.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs`: capture `var previousTotal = cart.Total;` before `cart.SetShippingMethod(...)`; after `ReplaceShippingAdjustment(...)` and before `cart.AdvanceCheckoutState(CheckoutState.Delivery)`, call `cart.RegressCheckoutIfAmountChanged(previousTotal)`. The advance then becomes a forward step from `Address` or an idempotent no-op at `Delivery`. | |  |
| TASK-005 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs`: capture `var previousTotal = cart.Total;` before `cart.UpdateDetails(...)`; after `cart.RecalculateTotals()` and before `SaveChangesAsync`, call `cart.RegressCheckoutIfAmountChanged(previousTotal)`, then `if (cart.HasAddresses() && cart.CheckoutState == CheckoutState.Address) { var adv = cart.AdvanceCheckoutState(CheckoutState.Delivery); if (adv.IsFailure) return adv.Errors; }`. | |  |
| TASK-006 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`: capture `var previousTotal = cart.Total;` before `lineItem.UpdateQuantity(...)`; after `cart.RecalculateTotals()` (line ~77) and before `SaveChangesAsync`, call `cart.RegressCheckoutIfAmountChanged(previousTotal)`. | |  |
| TASK-007 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs`: capture `var previousTotal = cart.Total;` before `cart.RemoveLineItem(...)`; after `dbContext.Set<LineItem>().Remove(...)` and before `SaveChangesAsync`, call `cart.RegressCheckoutIfAmountChanged(previousTotal)`. | |  |
| TASK-008 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`: capture `var previousTotal = cart.Total;` before the line item is added; call `cart.RegressCheckoutIfAmountChanged(previousTotal)` after the add/recalc and before `SaveChangesAsync`. | |  |

### Implementation Phase 3: Enrich cart response

- GOAL-003: Expose checkout prefill fields in the cart response and mirror them in the Store SPA schema.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.Base.cs`, add `Guid? ShippingMethodId { get; init; }`, `Guid? ShipAddressId { get; init; }`, `string? Email { get; init; }` to `CartResponseBase`. In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs`, extend `MapToDetail<T>` with `ShippingMethodId = entity.ShippingMethodId, ShipAddressId = entity.ShipAddressId, Email = entity.Email`. | |  |
| TASK-010 | In `app/Store/src/features/ordering/types/cart.ts`, add `shippingMethodId: string \| null`, `shipAddressId: string \| null`, `email: string \| null` to `CartResponse`. In `app/Store/src/features/ordering/validations/cart.ts`, add `shippingMethodId: z.string().nullable()`, `shipAddressId: z.string().nullable()`, `email: z.string().nullable()` to `CartResponseSchema`. | |  |

### Implementation Phase 4: Store SPA state sync

- GOAL-004: Drive the checkout step from the backend `checkoutState` and clear payment intent state on regression.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | In `app/Store/src/features/ordering/composables/useCart.ts`: add module-level refs `checkoutState = ref<string \| null>(null)`, `shippingMethodId = ref<string \| null>(null)`, `shipAddressId = ref<string \| null>(null)`, `email = ref<string \| null>(null)`; in `fetchCart` set them from `result.value` on success; add all four to the returned reactive object in `createCart()`. | |  |
| TASK-012 | In `app/Store/src/features/ordering/composables/useCheckout.ts`: import `watch`. Extend the local `CartRef` interface with `checkoutState: string \| null`. Replace the `currentStep` ref with (a) `backendStep` computed from `getCart().checkoutState` using PAT-001 mapping and (b) `displayStep = ref<Step>(1)`. Update `steps` computed to use `backendStep` for `complete` (`backendStep > N`) and `displayStep` for `current`. Remove optimistic writes: `saveAddress` sets `displayStep = 2` on success then `await getCart().fetchCart(true)`; `selectShippingRate` sets `displayStep = 3` on success then `await getCart().fetchCart(true)`; `placeOrder` sets `displayStep = 5` on success (no re-fetch). Add a helper `stepOf(state: string \| null): Step` that maps PAT-001 values with unknown/null/empty → `1`. Add `watch(() => getCart().checkoutState, (cur, prev) => { const prevStep = stepOf(prev); const curStep = stepOf(cur); if (prev === 'Payment' && cur === 'Delivery') { paymentClientSecret.value = null; paymentIntentId.value = null; paymentMethodId.value = null; } if (curStep >= 2 && curStep < (prevStep >= 2 ? prevStep : Number.MAX_SAFE_INTEGER)) { displayStep.value = curStep; } })`. Update `reset()` to also reset `displayStep = 1`. Return `backendStep`, `displayStep`, `steps` and the actions; remove `currentStep` from the returned object. | |  |
| TASK-013 | In `app/Store/src/features/ordering/views/CheckoutView.vue`: replace every `checkout.currentStep` reference. `goToStep` (~lines 224-228): set `checkout.displayStep` only when `value >= 1 && value <= Math.max(checkout.displayStep, checkout.backendStep)`; write `checkout.displayStep = value as CheckoutStep`. Stepper `:value="checkout.displayStep"` (line ~273). Step-3 `watch` (~lines 197-213): watch `() => checkout.displayStep`; on entry to step 3 resolve the payment method, and create the intent only when `!checkout.paymentClientSecret && checkout.backendStep === 2` (skip auto-create when the backend is already at `Payment` — reload case — and surface `checkout.error = 'Payment needs to be re-initiated. Go back and re-save your shipping method.'`); drop the `checkout.currentStep = 3` re-set. `onMounted` (~lines 248-259): after `await cart.fetchCart()`, seed `checkout.displayStep = Math.min(5, checkout.backendStep) as CheckoutStep`, prefill `selectedShippingId.value = cart.shippingMethodId`, `email.value = cart.email ?? auth.user?.email ?? ''`, and use `checkout.displayStep !== 5` in the empty-cart bounce guard. | |  |

### Implementation Phase 5: Tests + verification

- GOAL-005: Prove correctness with new unit tests and existing quality gates.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Add state-machine unit tests to `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` (xUnit + FluentAssertions, same `[Trait]` style): (1) `AdvanceCheckoutState` with `target == CheckoutState` returns success and leaves state unchanged; (2) `RegressCheckoutIfAmountChanged` regresses `Payment → Delivery` when total differs; (3) no regress when total unchanged; (4) no regress when `Status != Draft`; (5) `SetShippingMethod` on a cart at `Payment` with a changed rate regresses to `Delivery`. | |  |
| TASK-015 | Update `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts` to assert: step hydrates from backend `checkoutState` (`'Delivery'` → panel 2); re-selecting a shipping method on the same step succeeds; a regression `Payment → Delivery` clears `paymentClientSecret`/`paymentIntentId`; `goToStep` cannot advance beyond `backendStep`; empty-cart bounce is skipped when `displayStep === 5`. | |  |
| TASK-016 | Run and fix any failures before finishing: `dotnet build` (zero warnings); `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`; `cd app/Store && pnpm run lint && pnpm run test:unit`; `bash scripts/check-feature-conventions.sh`. Do not run `scripts/check-cross-module-refs.sh` for unrelated pre-existing drift. | |  |

## 3. Alternatives

- **ALT-001**: Derive the step purely from data completeness (address set → Delivery, method set → Payment, etc.) and drop the stored `CheckoutState` enum. Rejected: a much larger refactor that breaks cross-module Billing guards (`CreatePaymentIntent` reads `State`, `CreateOrderFromCart` reads `CheckoutState`); keeping the stored enum with a monotonic + regression rule achieves the same UX with far smaller blast radius.
- **ALT-002**: Keep strict forward-only transitions and add an explicit `regress` endpoint the SPA must call before re-editing. Rejected: two round-trips per edit and state drift risk if the SPA misses the call; a domain-level regression triggered by an amount change is self-healing.
- **ALT-003**: Return `checkoutState` from `PATCH /api/storefront/cart` and `PATCH /api/storefront/cart/shipping-rate` so the SPA avoids a re-fetch. Rejected by decision: changes the API contract; re-fetching the cart reuses the existing `GET /api/storefront/cart` shape and also refreshes totals/items.
- **ALT-004**: Make `AdvanceCheckoutState` a silent no-op for every target at or behind the current state. Rejected: masks programming errors in callers; explicit same-state idempotency (REQ-002) plus the explicit regression rule (REQ-003) keeps backward intent legible.

## 4. Dependencies

- **DEP-001**: Existing `Order` domain partial class `Order.Method.Checkout.cs` — home of `AdvanceCheckoutState`, `SetShippingMethod`, and the new `RegressCheckoutIfAmountChanged` (TASK-001..003).
- **DEP-002**: Existing `CartResponseBase` and `CartMapping.MapToDetail<T>` — extended in TASK-009 and consumed by `GET /api/storefront/cart`.
- **DEP-003**: Store SPA `useCart` singleton and `cart.fetchCart(true)` — consumed by TASK-012/TASK-013.
- **DEP-004**: Store SPA zod `CartResponseSchema` (`validations/cart.ts`) — must accept the new nullable fields (TASK-010) before `CartResponse` hydration (TASK-011) or the parse strips them.
- **DEP-005**: Existing `Order.Method.Tests.cs` and `CheckoutView.spec.ts` — extended in TASK-014/TASK-015.
- **DEP-006**: Follow-up Billing plan (deferred, CON-006) — void old unpaid `PaymentCapture`/gateway intent, widen `CreatePaymentIntent` guard to `state >= Delivery && != Confirm/Complete`, add `p.Amount == cart.Total` check in `CreateOrderFromCart`; not required for this plan's correctness of step/state display.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` — modify (TASK-001..003).
- **FILE-002**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs` — modify (TASK-004).
- **FILE-003**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` — modify (TASK-005).
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` — modify (TASK-006).
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs` — modify (TASK-007).
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — modify (TASK-008).
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.Base.cs` — modify (TASK-009).
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` — modify (TASK-009).
- **FILE-009**: `app/Store/src/features/ordering/types/cart.ts` — modify (TASK-010).
- **FILE-010**: `app/Store/src/features/ordering/validations/cart.ts` — modify (TASK-010).
- **FILE-011**: `app/Store/src/features/ordering/composables/useCart.ts` — modify (TASK-011).
- **FILE-012**: `app/Store/src/features/ordering/composables/useCheckout.ts` — modify (TASK-012).
- **FILE-013**: `app/Store/src/features/ordering/views/CheckoutView.vue` — modify (TASK-013).
- **FILE-014**: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` — modify (TASK-014).
- **FILE-015**: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts` — modify (TASK-015).

## 6. Testing

- **TEST-001**: Backend build — `dotnet build` passes with zero warnings (CON-001).
- **TEST-002**: Ordering unit tests — `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"` all pass.
- **TEST-003**: New state-machine unit tests from TASK-014 all pass (idempotent advance, amount-change regression, no-regress-when-unchanged, Draft-only regression, `SetShippingMethod` regression).
- **TEST-004**: Store SPA — `pnpm run lint` passes with zero warnings (touches `useCart.ts`, `useCheckout.ts`, `CheckoutView.vue`, `validations/cart.ts`, `types/cart.ts`).
- **TEST-005**: Store SPA — `pnpm run test:unit` passes (includes updated `CheckoutView.spec.ts`).
- **TEST-006**: Feature conventions — `bash scripts/check-feature-conventions.sh` passes (no feature-file drift).
- **TEST-007**: Manual smoke (optional, requires running API) — `ApiTests/Ordering/demo-flow.http`, plus: select shipping, refresh → SPA lands on Delivery; go back to Address and change address → proceeds; change shipping method on the Delivery step → no error; a `Payment → Delivery` regression clears intent state and step 3 re-creates the intent.

## 7. Risks & Assumptions

- **RISK-001**: Regression is silent — an amount-affecting edit mid-checkout silently drops the payment step; mitigated by the SPA watch (TASK-012) clearing intent refs, the step-3 watch re-creating the intent on re-entry, and `advanceToReview` re-validating.
- **RISK-002**: The `Total != previousTotal` comparison can trigger on decimal rounding even for semantically unchanged totals; mitigated by comparing against the same order's pre-mutation total and by the TASK-014 regression tests.
- **RISK-003**: The old unpaid payment intent/`PaymentCapture` is not voided in this plan (Billing out of scope, CON-006) — orphaned captures accumulate until the Billing follow-up (DEP-006) lands; a user reloading at `Payment` cannot re-obtain the client secret without a Billing change, and the SPA surfaces a guidance message instead of a hard error.
- **RISK-004**: The SPA regression watch could double-create an intent if a re-fetch races; mitigated by the existing `!checkout.paymentClientSecret` guard in the step-3 watch and by clearing the refs before the intent is re-created.
- **ASSUMPTION-001**: The Billing follow-up (void old intent, widen `CreatePaymentIntent` guard, amount-mismatch guard) will be a separate plan; this plan does not depend on it for correctness of step/state display (RISK-003 is the only visible gap).
- **ASSUMPTION-002**: `GET /api/storefront/cart` is the only hydration source the SPA needs; no new checkout-state endpoint is required.
- **ASSUMPTION-003**: The `Confirm → 4` mapping is effectively unreachable for a Draft order (placement goes `Payment → Confirm → Complete` atomically) and is kept only for completeness; the SPA Review panel (step 4) is set explicitly by `advanceToReview`.
- **ASSUMPTION-004**: No existing test asserts that `AdvanceCheckoutState` rejects a same-state transition or that `SetShippingMethod` preserves `CheckoutState`; if one does, it is updated in TASK-014/TASK-015.
- **ASSUMPTION-005**: The Confirmation panel (step 5) is in-session only; a refresh after placement returns the (now empty) cart to the cart page via the existing bounce guard.

## 8. Related Specifications / Further Reading

- [refactor-ordering-flow-1.md](./refactor-ordering-flow-1.md) — predecessor plan covering the checkout step race and Review validation.
- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md)
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md)
- [AGENTS.md](../AGENTS.md) — non-negotiable rules (result objects, module isolation, vertical-slice features, no destructive git).
