# Implementation Plan: Stripe Fixes

**Spec:** `docs/superpowers/specs/2026-08-07-stripe-fixes-design.md`
**Estimated effort:** Medium (2-3 hours)
**Dependencies:** None

## Tasks

### Fix 1: Call confirmPayment() after client success
- [ ] Edit `app/Store/src/features/ordering/stores/checkoutStore.ts`
- [ ] Add `confirmPayment(paymentId)` action that calls `paymentApi.confirmPayment(paymentId)`
- [ ] Edit `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`
- [ ] After `stripe.confirmCardPayment()` succeeds, call `checkout.confirmPayment(paymentId)`
- [ ] Then advance to step 4

### Fix 2: Add ReturnUrl for 3DS
- [ ] Edit `app/Store/src/features/ordering/stores/checkoutStore.ts`
- [ ] In `createPaymentIntent()`, add `returnUrl: window.location.origin + '/checkout'` to request
- [ ] Update `CreatePaymentIntentRequest` type to include `returnUrl`

### Fix 3: Clean dead fields
- [ ] Edit `app/Store/src/features/ordering/types/checkout.ts`
- [ ] Remove `amount` and `currency` from `CreatePaymentIntentRequest`

### Fix 4: Remove dead function
- [ ] Edit `app/Store/src/features/payment/services/paymentApi.ts`
- [ ] Remove `createPaymentIntent()` function (lines 19-21)

### Fix 5: Add dev Stripe key
- [ ] Edit `app/Store/.env.development`
- [ ] Add `VITE_STRIPE_PUBLISHABLE_KEY=pk_test_placeholder`

### Fix 6: Add rate limiting to SetupIntent
- [ ] Edit `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs`
- [ ] Add `.RequireRateLimiting("payment")` to endpoint

### T7: Verify
- [ ] confirmPayment() called after stripe.confirmCardPayment()
- [ ] ReturnUrl included in PaymentIntent creation
- [ ] Dead fields removed
- [ ] Dev Stripe key set
- [ ] SetupIntent has rate limiting

## Verification

```bash
cd service/Api && dotnet build
cd app/Store && pnpm run lint && pnpm run test:unit
```
