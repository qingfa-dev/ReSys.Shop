# Stripe Fixes

## Summary

6 targeted fixes to the Stripe payment integration without restructuring the payment flow.

## Current Issues

1. After `stripe.confirmCardPayment()` succeeds, frontend never calls `confirmPayment()` API
2. No `ReturnUrl` set on PaymentIntent — 3DS cards fail
3. Dead `amount`/`currency` fields in frontend `CreatePaymentIntentRequest` type
4. `paymentApi.ts:createPaymentIntent()` never called (dead code)
5. `.env.development` has empty Stripe publishable key
6. `SetupIntent` endpoint missing rate limiting

## Fixes

### Fix 1: Call confirmPayment() After Client Success

**File:** `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`

**Current (lines 71-77):**
```ts
const { error } = await stripe.confirmCardPayment(clientSecret)
if (error) { /* handle error */ }
else { checkout.goToStep(4) }
```

**After:**
```ts
const { error } = await stripe.confirmCardPayment(clientSecret)
if (error) { /* handle error */ }
else {
  await checkout.confirmPayment(paymentId)  // NEW: sync local state
  checkout.goToStep(4)
}
```

**checkoutStore.ts addition:**
```ts
async confirmPayment(paymentId: string) {
  await paymentApi.confirmPayment(paymentId)
}
```

### Fix 2: Add ReturnUrl for 3DS

**File:** `app/Store/src/features/ordering/stores/checkoutStore.ts`

**Current (lines 103-108):** sends `{ orderId, amount, currency, paymentMethodId }`

**After:** sends `{ orderId, paymentMethodId, returnUrl: window.location.origin + '/checkout' }`

### Fix 3: Clean Dead Fields

**File:** `app/Store/src/features/ordering/types/checkout.ts`

**Remove** `amount` and `currency` from `CreatePaymentIntentRequest`:
```ts
// Before
interface CreatePaymentIntentRequest {
  orderId: string
  amount: number      // REMOVE
  currency: string    // REMOVE
  paymentMethodId: string
}

// After
interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId: string
}
```

### Fix 4: Remove Dead Function

**File:** `app/Store/src/features/payment/services/paymentApi.ts`

**Remove** `createPaymentIntent()` function (lines 19-21) — never called from checkout flow.

### Fix 5: Add Dev Stripe Key

**File:** `app/Store/.env.development`

```
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_placeholder
```

### Fix 6: Add Rate Limiting to SetupIntent

**File:** `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs`

**Add** `.RequireRateLimiting("payment")` to match `CreatePaymentIntent` and `ConfirmPayment` endpoints.

## Files to Modify

| File | Fix |
|------|-----|
| `features/ordering/components/CheckoutStepPayment.vue` | Fix 1 |
| `features/ordering/stores/checkoutStore.ts` | Fix 1, 2 |
| `features/ordering/types/checkout.ts` | Fix 3 |
| `features/payment/services/paymentApi.ts` | Fix 4 |
| `.env.development` | Fix 5 |
| `Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs` | Fix 6 |

## Acceptance Criteria

- [ ] confirmPayment() called after stripe.confirmCardPayment() succeeds
- [ ] ReturnUrl included in PaymentIntent creation
- [ ] Dead amount/currency fields removed from types
- [ ] Dead createPaymentIntent function removed from paymentApi
- [ ] Dev Stripe key placeholder set
- [ ] SetupIntent endpoint has rate limiting
