# Storefront API Fixes + Checkout Wiring — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 4 API route mismatches in `api.ts`, wire checkout steps (Address, Delivery, Payment) to real backend services, and register Terms/Privacy routes.

**Architecture:** Surgical changes to `shared/constants/api.ts` fix route paths. Checkout step components rewritten to call existing service API functions instead of using hardcoded placeholder data. Stripe Elements integrated via existing `usePayment` composable. No new backend endpoints needed.

**Tech Stack:** Vue 3.5, TypeScript 6.0, PrimeVue 5, Stripe.js (`@stripe/stripe-js`), Axios.

## Global Constraints

- TypeScript `noUncheckedIndexedAccess: true` enforced
- `pnpm run type-check` — 0 errors after each task
- `pnpm run lint` — 0 violations after each task
- `pnpm run test:unit -- --run` — existing tests still pass after each task
- All API calls use existing `ENDPOINTS` constant and existing service functions
- No mock data — all checkout data comes from real backend API responses
- Stripe publishable key read from `import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY`

---

### Task 1: Fix API route mismatches in constants

**Files:**
- Modify: `app/Store/src/shared/constants/api.ts`
- Modify: `app/Store/src/features/location/services/countryApi.ts`
- Modify: `app/Store/src/features/location/composables/useLocationCascade.ts`

**Interfaces:**
- Produces: corrected endpoint URLs
- Consumes: nothing (replaces existing broken constants)

- [ ] **Step 1: Fix profile double-segment (line 69)**

Read `app/Store/src/shared/constants/api.ts`. Line 69 currently reads:
```ts
profiles: `${API_STORE}/profiles/profiles`,
```
Change to:
```ts
profiles: `${API_STORE}/profiles`,
```

Also remove the misleading comment on lines 67-68:
```
// NOTE: the backend store profile routes live under api/store/profiles/profiles
// (the plan's bare api/store/profiles would 404). Set-default uses PUT address.
```
Replace with:
```
// Profile CRUD — backend route: /api/store/profiles
```

- [ ] **Step 2: Fix location idOrIso to typed dual functions (lines 80-82)**

Replace:
```ts
countryByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/countries/${idOrIso}`,
states: `${API_STORE}/locations/states`,
stateByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/states/${idOrIso}`,
```
With:
```ts
countryById: (id: string) => `${API_STORE}/locations/countries/${id}`,
countryByIso: (iso: string) => `${API_STORE}/locations/countries/by-iso/${iso}`,
states: `${API_STORE}/locations/states`,
stateById: (id: string) => `${API_STORE}/locations/states/${id}`,
stateByIso: (iso: string) => `${API_STORE}/locations/states/by-iso/${iso}`,
```

- [ ] **Step 3: Comment out dead routes (lines 12, 40)**

Line 12: Comment out `taxonomies`:
```ts
// taxonomies: No backend GET /api/storefront/taxonomies list endpoint
```

Line 40: Comment out `sessionById`:
```ts
// sessionById: Backend route not yet available — use GET /sessions for list
```

- [ ] **Step 4: Update call sites for renamed functions**

Search for all usages of the old names:
```bash
rg "countryByIdOrIso|stateByIdOrIso" app/Store/src/
```

Expected: hits in `countryApi.ts` and `useLocationCascade.ts`.

Update `app/Store/src/features/location/services/countryApi.ts`:
- Change any `ENDPOINTS.countryByIdOrIso(id)` to `ENDPOINTS.countryById(id)` or `ENDPOINTS.countryByIso(iso)` based on whether the caller passes a GUID or ISO code
- If the function signature accepts a single ambiguous parameter, split into two separate functions: `getCountryById(id: string)` and `getCountryByIso(iso: string)`

Update `app/Store/src/features/location/composables/useLocationCascade.ts`:
- Replace old endpoint references with new names

- [ ] **Step 5: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors after all call sites updated. If any unresolved references to old names, fix them.

- [ ] **Step 6: Verify no old names remain**

```bash
rg "countryByIdOrIso|stateByIdOrIso" app/Store/src/
```
Expected: 0 results.

```bash
rg "profiles/profiles" app/Store/src/
```
Expected: 0 results.

- [ ] **Step 7: Commit**

```bash
git add app/Store/src/shared/constants/api.ts app/Store/src/features/location/services/countryApi.ts app/Store/src/features/location/composables/useLocationCascade.ts
git commit -m "fix(store): correct API route mismatches in profiles, locations, sessions"
```

---

### Task 2: Wire CheckoutStepAddress to real address API

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`

**Interfaces:**
- Consumes: `addressApi.getAddresses()`, `checkoutStore.saveAddress()`
- Produces: address selection step using user's real address book

- [ ] **Step 1: Read and verify needed types**

Read `app/Store/src/features/profile/types/address.ts` to verify the `Address` type. Note the fields: `id`, `fullName`, `addressLine1`, `addressLine2`, `city`, `state`, `country`, `phone`, `postalCode`, `isDefault`.

Read `app/Store/src/features/profile/services/addressApi.ts` to verify `getAddresses()` function signature. Expected: `getAddresses() -> Promise<Result<Address[]>>` or `-> Promise<PagedResult<Address>>`.

- [ ] **Step 2: Rewrite CheckoutStepAddress.vue**

Write `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCheckoutStore } from '../stores/checkoutStore'
import { getAddresses } from '@/features/profile/services/addressApi'
import type { Address } from '@/features/profile/types/address'

const checkout = useCheckoutStore()
const auth = useAuthStore()

const addresses = ref<Address[]>([])
const selectedAddressId = ref<string | null>(null)
const email = ref(auth.user?.email ?? '')
const loading = ref(true)
const localError = ref<string | null>(null)

// Trigger: Fetch user addresses on mount.
onMounted(async () => {
  const result = await getAddresses()
  if (result.isSuccess) {
    addresses.value = Array.isArray(result.value) ? result.value : (result.value as any)?.items ?? []
  } else {
    localError.value = result.message ?? 'Failed to load addresses'
  }
  loading.value = false
})

// Trigger: Save selected address and advance to delivery step.
async function continueToDelivery(): Promise<void> {
  if (!selectedAddressId.value) return
  const saved = await checkout.saveAddress(selectedAddressId.value, email.value)
  if (saved) await checkout.goToStep(2)
}
</script>
<template>
  <!-- Section: Address Step -->
  <div class="bg-white rounded-xl border border-stone-200 p-6">
    <h2 class="text-lg font-semibold text-stone-900 mb-4">Shipping Address</h2>

    <!-- Section: Email -->
    <div class="mb-4">
      <label class="block text-sm font-medium text-stone-700 mb-1" for="checkout-email">Email</label>
      <InputText id="checkout-email" v-model="email" type="email" class="w-full" />
    </div>

    <!-- Section: Loading -->
    <div v-if="loading" class="space-y-3 mb-6">
      <div v-for="i in 3" :key="i" class="h-16 bg-stone-100 rounded-lg animate-pulse" />
    </div>

    <!-- Section: Error -->
    <Message v-else-if="localError" severity="error" class="mb-6">{{ localError }}</Message>

    <!-- Section: Empty -->
    <div v-else-if="addresses.length === 0" class="mb-6">
      <p class="text-sm text-stone-500 mb-3">No saved addresses. Please add one.</p>
      <router-link to="/account/addresses">
        <Button label="Go to Address Book" severity="secondary" size="small" />
      </router-link>
    </div>

    <!-- Section: Address List -->
    <div v-else class="mb-6 space-y-2">
      <div
        v-for="addr in addresses"
        :key="addr.id"
        class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors"
        :class="selectedAddressId === addr.id ? 'border-teal-600 bg-teal-50' : 'border-stone-200 hover:border-stone-300'"
        @click="selectedAddressId = addr.id"
      >
        <RadioButton v-model="selectedAddressId" :input-id="`addr-${addr.id}`" :value="addr.id" />
        <label :for="`addr-${addr.id}`" class="flex-1 text-sm cursor-pointer">
          <span class="font-medium text-stone-900">{{ addr.fullName }}</span>
          <span class="text-stone-500 ml-2">{{ addr.addressLine1 }}, {{ addr.city }}{{ addr.state ? `, ${addr.state}` : '' }}{{ addr.country ? `, ${addr.country}` : '' }}</span>
        </label>
      </div>
    </div>

    <!-- Section: Actions -->
    <div class="flex justify-end">
      <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" :disabled="!selectedAddressId || checkout.loading" @click="continueToDelivery" />
    </div>
  </div>
</template>
```

- [ ] **Step 3: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors. `RadioButton` auto-imported by PrimeVue.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/ordering/components/CheckoutStepAddress.vue
git commit -m "feat(store): wire CheckoutStepAddress to real address API"
```

---

### Task 3: Wire CheckoutStepDelivery to real shipping API

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`

**Interfaces:**
- Consumes: `shippingApi.getShippingMethods()`, `checkoutStore.calculateShipping()`
- Produces: shipping method selection step

- [ ] **Step 1: Verify shipping types**

Read `app/Store/src/features/shipping/types/shipping.ts` to verify `ShippingMethod` interface. If missing fields like `carrier` or `estimatedDays`, extend the interface after verifying actual API response from backend.

Read `app/Store/src/features/shipping/services/shippingApi.ts` to verify `getShippingMethods()` signature.

- [ ] **Step 2: Rewrite CheckoutStepDelivery.vue**

Write `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useCheckoutStore } from '../stores/checkoutStore'
import { getShippingMethods } from '@/features/shipping/services/shippingApi'
import type { ShippingMethod } from '@/features/shipping/types/shipping'

const checkout = useCheckoutStore()

const methods = ref<ShippingMethod[]>([])
const selectedMethodId = ref<string | null>(null)
const loading = ref(true)
const localError = ref<string | null>(null)

// Trigger: Fetch shipping methods on mount.
onMounted(async () => {
  const result = await getShippingMethods()
  if (result.isSuccess) {
    methods.value = Array.isArray(result.value) ? result.value : (result.value as any)?.items ?? []
    if (methods.value.length === 1) selectedMethodId.value = methods.value[0]?.id ?? null
  } else {
    localError.value = result.message ?? 'Failed to load shipping methods'
  }
  loading.value = false
})

// Trigger: Select shipping method and advance to payment step.
async function continueToPayment(): Promise<void> {
  if (!selectedMethodId.value) return
  const calculated = await checkout.calculateShipping(selectedMethodId.value)
  if (calculated) await checkout.goToStep(3)
}
</script>
<template>
  <!-- Section: Delivery Step -->
  <div class="bg-white rounded-xl border border-stone-200 p-6">
    <h2 class="text-lg font-semibold text-stone-900 mb-4">Delivery Method</h2>

    <!-- Section: Loading -->
    <div v-if="loading" class="space-y-3 mb-6">
      <div v-for="i in 2" :key="i" class="h-16 bg-stone-100 rounded-lg animate-pulse" />
    </div>

    <!-- Section: Error -->
    <Message v-else-if="localError" severity="error" class="mb-6">{{ localError }}</Message>

    <!-- Section: No Methods -->
    <p v-else-if="methods.length === 0" class="text-sm text-stone-500 mb-6">No shipping methods available for your location.</p>

    <!-- Section: Shipping Methods -->
    <div v-else class="mb-6 space-y-2">
      <div
        v-for="method in methods"
        :key="method.id"
        class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors"
        :class="selectedMethodId === method.id ? 'border-teal-600 bg-teal-50' : 'border-stone-200 hover:border-stone-300'"
        @click="selectedMethodId = method.id"
      >
        <RadioButton v-model="selectedMethodId" :input-id="`ship-${method.id}`" :value="method.id" />
        <label :for="`ship-${method.id}`" class="flex-1 text-sm cursor-pointer">
          <span class="font-medium text-stone-900">{{ method.name }}</span>
          <span v-if="method.carrier" class="text-stone-500 ml-2">{{ method.carrier }}</span>
        </label>
      </div>
    </div>

    <!-- Section: Actions -->
    <div class="flex justify-between">
      <Button label="Back" icon="pi pi-arrow-left" severity="secondary" :disabled="checkout.loading" @click="checkout.goToStep(1)" />
      <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" :disabled="!selectedMethodId || checkout.loading" @click="continueToPayment" />
    </div>
  </div>
</template>
```

- [ ] **Step 3: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/ordering/components/CheckoutStepDelivery.vue
git commit -m "feat(store): wire CheckoutStepDelivery to real shipping API"
```

---

### Task 4: Wire CheckoutStepPayment to real payment API + Stripe Elements

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`

**Interfaces:**
- Consumes: `paymentApi.getPaymentMethods()`, `usePayment` composable, `checkoutStore.createPaymentIntent()`
- Produces: payment step with Stripe Card Element

- [ ] **Step 1: Verify payment types and composable**

Read `app/Store/src/features/payment/types/payment.ts` to verify `PaymentMethod` type.

Read `app/Store/src/features/payment/services/paymentApi.ts` to verify `getPaymentMethods()`, `createPaymentIntent()`, `confirmPayment()` signatures.

Read `app/Store/src/features/payment/composables/usePayment.ts` to verify API: `init(publishableKey)`, `mount(clientSecret, container)`, `unmount()`, `stripePromise`, `loading`, `error`.

- [ ] **Step 2: Verify Stripe publishable key env var**

Read `app/Store/.env.development`. If `VITE_STRIPE_PUBLISHABLE_KEY` is not defined, add:
```
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_placeholder
```

- [ ] **Step 3: Rewrite CheckoutStepPayment.vue**

Write `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useCartStore } from '../stores/cartStore'
import { useCheckoutStore } from '../stores/checkoutStore'
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import { usePayment } from '@/features/payment/composables/usePayment'
import { formatVnd } from '@/shared/utils/currency'
import type { PaymentMethod } from '@/features/payment/types/payment'

const checkout = useCheckoutStore()
const cart = useCartStore()
const payment = usePayment()

const methods = ref<PaymentMethod[]>([])
const selectedMethodId = ref<string | null>(null)
const loading = ref(true)
const processing = ref(false)
const localError = ref<string | null>(null)
const cardContainer = ref<HTMLElement | null>(null)
const clientSecret = ref<string | null>(null)

// Map: Stripe publishable key from env.
const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY as string

// Trigger: Fetch payment methods on mount.
onMounted(async () => {
  const result = await getPaymentMethods()
  if (result.isSuccess) {
    methods.value = Array.isArray(result.value) ? result.value : (result.value as any)?.items ?? []
  } else {
    localError.value = result.message ?? 'Failed to load payment methods'
  }
  loading.value = false
  if (publishableKey && publishableKey !== 'pk_test_placeholder') {
    payment.init(publishableKey)
  }
})

// Clean up Stripe Elements on unmount.
onUnmounted(() => payment.unmount())

// Trigger: Create payment intent and mount Stripe card.
async function createIntent(): Promise<void> {
  if (!selectedMethodId.value || !cart.id) return
  processing.value = true
  localError.value = null
  const secret = await checkout.createPaymentIntent(selectedMethodId.value, cart.subtotal)
  processing.value = false
  if (secret) {
    clientSecret.value = secret
    if (cardContainer.value && payment.stripePromise) {
      await payment.mount(secret, cardContainer.value)
    }
  } else {
    localError.value = checkout.error ?? 'Unable to create a payment intent.'
  }
}

// Trigger: Confirm payment with Stripe and advance to confirm step.
async function pay(): Promise<void> {
  if (!clientSecret.value) return
  processing.value = true
  localError.value = null
  const stripe = await payment.stripePromise
  if (!stripe) {
    localError.value = 'Stripe is not available. Please try again later.'
    processing.value = false
    return
  }
  const { error } = await stripe.confirmCardPayment(clientSecret.value)
  if (error) {
    localError.value = error.message ?? 'Payment failed. Please try again.'
    processing.value = false
  } else {
    await checkout.goToStep(4)
    processing.value = false
  }
}
</script>
<template>
  <!-- Section: Payment Step -->
  <div class="bg-white rounded-xl border border-stone-200 p-6">
    <h2 class="text-lg font-semibold text-stone-900 mb-4">Payment</h2>
    <div class="flex justify-between text-sm text-stone-600 mb-4">
      <span>Order total</span>
      <span class="font-semibold text-stone-900">{{ formatVnd(cart.subtotal) }}</span>
    </div>

    <!-- Section: Loading -->
    <div v-if="loading" class="space-y-3 mb-6">
      <div v-for="i in 2" :key="i" class="h-16 bg-stone-100 rounded-lg animate-pulse" />
    </div>

    <!-- Section: Error -->
    <Message v-else-if="localError" severity="error" class="mb-6">{{ localError }}</Message>

    <!-- Section: No Methods -->
    <p v-else-if="methods.length === 0" class="text-sm text-stone-500 mb-6">No payment methods available.</p>

    <!-- Section: Payment Methods -->
    <div v-else class="mb-6">
      <span class="block text-sm font-medium text-stone-700 mb-2">Payment method</span>
      <div class="space-y-2">
        <div
          v-for="opt in methods"
          :key="opt.id"
          class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors"
          :class="selectedMethodId === opt.id ? 'border-teal-600 bg-teal-50' : 'border-stone-200 hover:border-stone-300'"
          @click="selectedMethodId = opt.id"
        >
          <RadioButton v-model="selectedMethodId" :input-id="`pay-${opt.id}`" :value="opt.id" />
          <label :for="`pay-${opt.id}`" class="text-sm text-stone-700 cursor-pointer">{{ opt.name }}</label>
        </div>
      </div>
      <!-- Section: Proceed to Card Input -->
      <Button v-if="selectedMethodId && !clientSecret" label="Continue to Payment" icon="pi pi-credit-card" class="mt-4 w-full" :loading="processing" @click="createIntent" />
    </div>

    <!-- Section: Stripe Card Element -->
    <div v-if="clientSecret" class="mb-6">
      <p class="text-sm text-stone-500 mb-3">Enter your card details:</p>
      <div ref="cardContainer" class="p-4 border border-stone-200 rounded-lg min-h-[40px]" />
    </div>

    <!-- Section: Actions -->
    <div v-if="clientSecret" class="flex justify-between">
      <Button label="Back" icon="pi pi-arrow-left" severity="secondary" :disabled="processing || checkout.loading" @click="checkout.goToStep(2)" />
      <Button label="Pay" icon="pi pi-credit-card" iconPos="right" :loading="processing" :disabled="!clientSecret" @click="pay" />
    </div>
  </div>
</template>
```

- [ ] **Step 4: Run type-check + lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors. `RadioButton` auto-imported by PrimeVue.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/ordering/components/CheckoutStepPayment.vue app/Store/.env.development
git commit -m "feat(store): wire CheckoutStepPayment to real payment API + Stripe Elements"
```

---

### Task 5: Register Terms + Privacy routes (if not done by Spec B)

**Files:**
- Modify: `app/Store/src/features/catalog/routes/index.ts`

**Interfaces:**
- Consumes: Vue Router route definitions
- Produces: /terms and /privacy routes

If Spec B (Task 8 Step 4) has already added these routes, skip this task.

- [ ] **Step 1: Check if routes already exist**

```bash
grep -n "terms\|privacy" app/Store/src/features/catalog/routes/index.ts
```
If both `/terms` and `/privacy` routes are present, skip to Step 4.

- [ ] **Step 2: Add route entries**

Read `app/Store/src/features/catalog/routes/index.ts`. Add before the closing `]`:

```ts
{
  path: '/terms',
  name: 'terms',
  component: () => import('../views/TermsView.vue'),
  meta: { title: 'Terms of Service' },
},
{
  path: '/privacy',
  name: 'privacy',
  component: () => import('../views/PrivacyView.vue'),
  meta: { title: 'Privacy Policy' },
},
```

If the view files don't exist yet (Spec B hasn't run), create placeholder views first.

- [ ] **Step 3: Create placeholder views if needed**

If `TermsView.vue` doesn't exist at `app/Store/src/features/catalog/views/TermsView.vue`, create a minimal placeholder:

```vue
<template>
  <div class="max-w-3xl mx-auto px-4 py-16">
    <h1>Terms of Service</h1>
    <p class="text-stone-600">Placeholder legal content.</p>
  </div>
</template>
```

Same pattern for `PrivacyView.vue` with "Privacy Policy" as title.

- [ ] **Step 4: Run type-check**

```bash
cd app/Store && pnpm run type-check
```
Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/catalog/routes/index.ts app/Store/src/features/catalog/views/TermsView.vue app/Store/src/features/catalog/views/PrivacyView.vue
git commit -m "feat(store): register Terms and Privacy routes"
```

---

### Task 6: Final verification

- [ ] **Step 1: Full test suite**

```bash
cd app/Store && pnpm run type-check && pnpm run lint && pnpm run test:unit -- --run
```
Expected: 0 errors, 0 violations, all tests pass.

- [ ] **Step 2: Verify route fixes against real backend**

Start the .NET backend (if available) or verify against the `.http` test files:

1. `curl http://localhost:5035/api/store/profiles` — returns 200 (not 404)
2. `curl http://localhost:5035/api/store/locations/countries/by-iso/VN` — returns country data
3. No `countryByIdOrIso` or `stateByIdOrIso` references in codebase
4. No `profiles/profiles` double path in codebase

- [ ] **Step 3: Manual checkout flow test**

```bash
cd app/Store && pnpm run dev &
```

1. Add product to cart -> go to checkout
2. Address step: shows addresses from API (or "No saved addresses" message)
3. Select address -> click Continue -> delivery step shows real shipping methods
4. Select method -> click Continue -> payment step shows real payment methods
5. Select method -> click "Continue to Payment" -> Stripe card field appears
6. /terms page loads (not 404)
7. /privacy page loads (not 404)

```bash
kill %1
```

---

## Verification

1. `pnpm run type-check` — 0 errors
2. `pnpm run lint` — 0 violations
3. `pnpm run test:unit -- --run` — all tests pass
4. `rg "countryByIdOrIso|stateByIdOrIso" src/` — 0 results
5. `rg "profiles/profiles" src/` — 0 results
6. Checkout flow: Address, Delivery, Payment steps use real API data
7. Stripe Card Element mounts and processes test card `4242 4242 4242 4242`
8. /terms and /privacy render content without 404
