# Stripe Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 6 targeted issues in Stripe payment integration: confirm sync, ReturnUrl, dead code cleanup, dev key, rate limiting.

**Architecture:** Surgical fixes to existing files. No restructuring. Each fix is isolated and independently verifiable.

**Tech Stack:** Vue 3, Stripe.js, .NET 10 Carter endpoints

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Backend: `.RequireRateLimiting("payment")` pattern for payment endpoints
- Stripe.js: `loadStripe(publishableKey)` singleton pattern
- All API calls return `Result<T>` objects

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/features/ordering/components/CheckoutStepPayment.vue` | MODIFY | Fix 1: confirm sync |
| `app/Store/src/features/ordering/stores/checkoutStore.ts` | MODIFY | Fix 1, 2: confirm + ReturnUrl |
| `app/Store/src/features/ordering/types/checkout.ts` | MODIFY | Fix 3: dead fields |
| `app/Store/src/features/payment/services/paymentApi.ts` | MODIFY | Fix 4: dead function |
| `app/Store/.env.development` | MODIFY | Fix 5: dev key |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs` | MODIFY | Fix 6: rate limiting |

---

## Tasks

### Task 1: Fix confirmPayment sync (Fix 1)

**Files:**
- Modify: `app/Store/src/features/ordering/stores/checkoutStore.ts`
- Modify: `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`

**Interfaces:**
- Consumes: `paymentApi.confirmPayment(paymentId)`
- Produces: `checkoutStore.confirmPayment(paymentId)` action

- [ ] **Step 1: Read current checkoutStore.ts**

Read `app/Store/src/features/ordering/stores/checkoutStore.ts` to understand existing structure.

- [ ] **Step 2: Add confirmPayment action**

After the `placeOrder` function (around line 140), add:

```typescript
async function confirmPayment(paymentId: string): Promise<void> {
  await paymentApi.confirmPayment(paymentId)
}
```

Add `confirmPayment` to the return object.

- [ ] **Step 3: Read CheckoutStepPayment.vue**

Read `app/Store/src/features/ordering/components/CheckoutStepPayment.vue` to find the `stripe.confirmCardPayment` success handler (around line 71-77).

- [ ] **Step 4: Add confirmPayment call after success**

After `stripe.confirmCardPayment()` succeeds (after the `if (error)` block), add:

```typescript
await checkout.confirmPayment(checkout.paymentId)
```

Before `checkout.goToStep(4)`.

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 2: Add ReturnUrl for 3DS (Fix 2)

**Files:**
- Modify: `app/Store/src/features/ordering/stores/checkoutStore.ts`
- Modify: `app/Store/src/features/ordering/types/checkout.ts`

**Interfaces:**
- Consumes: None
- Produces: Updated request type with `returnUrl`

- [ ] **Step 1: Add returnUrl to CreatePaymentIntentRequest**

Edit `app/Store/src/features/ordering/types/checkout.ts`. Add `returnUrl` to the request interface:

```typescript
export interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId: string
  returnUrl?: string
}
```

- [ ] **Step 2: Pass returnUrl in checkoutStore**

Edit `app/Store/src/features/ordering/stores/checkoutStore.ts`. In `createPaymentIntent()`, add `returnUrl` to the request:

```typescript
returnUrl: window.location.origin + '/checkout',
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Clean dead fields (Fix 3)

**Files:**
- Modify: `app/Store/src/features/ordering/types/checkout.ts`

**Interfaces:**
- Consumes: None
- Produces: Cleaner type definition

- [ ] **Step 1: Remove dead fields**

Edit `app/Store/src/features/ordering/types/checkout.ts`. Remove `amount` and `currency` from `CreatePaymentIntentRequest` (they were added in Task 2's step but the original dead fields should be removed):

The final interface should be:

```typescript
export interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId: string
  returnUrl?: string
}
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 4: Remove dead function (Fix 4)

**Files:**
- Modify: `app/Store/src/features/payment/services/paymentApi.ts`

**Interfaces:**
- Consumes: None
- Produces: Cleaner API service

- [ ] **Step 1: Read paymentApi.ts**

Read `app/Store/src/features/payment/services/paymentApi.ts` to find the unused `createPaymentIntent` function.

- [ ] **Step 2: Remove dead function**

Delete the `createPaymentIntent` function (around lines 19-21) that is never called from the checkout flow.

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 5: Add dev Stripe key (Fix 5)

**Files:**
- Modify: `app/Store/.env.development`

**Interfaces:**
- Consumes: None
- Produces: Configured dev environment

- [ ] **Step 1: Read .env.development**

Read `app/Store/.env.development` to find the current Stripe key line.

- [ ] **Step 2: Set placeholder key**

Ensure the file contains:

```
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_placeholder
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 6: Add rate limiting to SetupIntent (Fix 6)

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs`

**Interfaces:**
- Consumes: None
- Produces: Rate-limited endpoint

- [ ] **Step 1: Read the endpoint file**

Read `service/Api/src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs`.

- [ ] **Step 2: Add rate limiting**

Add `.RequireRateLimiting("payment")` to the endpoint builder chain, matching the pattern used in `CreatePaymentIntent.Endpoint.cs`.

- [ ] **Step 3: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/features/ordering/components/CheckoutStepPayment.vue src/features/ordering/stores/checkoutStore.ts src/features/ordering/types/checkout.ts src/features/payment/services/paymentApi.ts .env.development
cd service/Api && git add src/Module/Payment/Features/Storefront/Payment/SetupIntent/CreateSetupIntent.Endpoint.cs
git commit -m "fix(payment): confirm sync, ReturnUrl, dead code cleanup, rate limiting"
```
