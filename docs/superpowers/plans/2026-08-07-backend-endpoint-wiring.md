# Implementation Plan: Backend Endpoint Wiring

**Spec:** `docs/superpowers/specs/2026-08-07-backend-endpoint-wiring-design.md`
**Estimated effort:** Medium (3-4 hours)
**Dependencies:** None

## Tasks

### T1: Create default address endpoint (Backend)
- [ ] Create `Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.cs`
- [ ] Create `Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Endpoint.cs`
- [ ] Create `Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Response.cs`
- [ ] Route: `GET api/store/profiles/addresses/default`
- [ ] Auth: RequireAuthorization
- [ ] Handler: Query user addresses, return one with IsDefault == true, fallback to first

### T2: Wire default address in CheckoutStepAddress
- [ ] Edit `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`
- [ ] Add `getDefaultAddress()` API call in `paymentApi.ts` or `addressApi.ts`
- [ ] On mount, fetch default address
- [ ] Auto-select returned address
- [ ] Fallback to first address if empty

### T3: Wire setup-intent in CheckoutStepPayment
- [ ] Edit `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`
- [ ] Add "Save this card for future purchases" checkbox
- [ ] When checked, call `createSetupIntent({ paymentMethodId })` after payment success

### T4: Show delivery estimate in CheckoutStepDelivery
- [ ] Edit `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`
- [ ] Read `deliveryRange` from each shipping rate in existing list response
- [ ] Display "Est. delivery: Aug 15-17" next to each rate option

### T5: Add API functions
- [ ] Edit `app/Store/src/features/profile/services/addressApi.ts`
- [ ] Add `getDefaultAddress()` function
- [ ] Edit `app/Store/src/features/payment/services/paymentApi.ts`
- [ ] Verify `createSetupIntent()` exists and works

### T6: Verify
- [ ] Default address auto-selected in checkout
- [ ] "Save card" checkbox appears in payment step
- [ ] Delivery estimate shown per shipping rate
- [ ] No regressions in existing checkout flow

## Verification

```bash
cd service/Api && dotnet build && dotnet test
cd app/Store && pnpm run lint && pnpm run test:unit
```
