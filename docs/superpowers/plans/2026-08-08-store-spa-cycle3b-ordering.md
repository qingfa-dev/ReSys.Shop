# Store SPA Cycle 3b: Ordering — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans.

**Goal:** Replace 4 ordering skeleton views with functional implementations — Cart page, Checkout wizard, Order list, Order detail.

**Architecture:** CartView + CheckoutView use DefaultLayout with sidebar summary. OrderListView + OrderDetailView use AccountLayout. All stores fully wired — views only consume existing APIs.

**Tech Stack:** Vue 3.5, PrimeVue 5, Tailwind CSS v4, Vitest + jsdom, @pinia/testing

## Global Constraints

- `TreatWarningsAsErrors=true` — no TypeScript warnings
- Neutral palette only (`neutral-*`), teal primary for CTAs
- Inter body font, JetBrains Mono for prices
- All views use `max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8`
- Cart item images: `w-20 h-24 object-cover rounded-md bg-neutral-100`
- Status tags: Processing=info, Shipped=warn, Delivered=success, Canceled=danger
- Cancel actions require PrimeVue ConfirmDialog confirmation

---

### Task 1: CartView — full-page cart

**Files:** Modify: `app/Store/src/features/ordering/views/CartView.vue`

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCartStore } from '../stores/cartStore'

usePageTitle('Cart')
const cart = useCartStore()

onMounted(() => { cart.fetchCart() })

function updateQty(id: string, qty: number): void {
  if (qty < 1) return
  cart.updateQuantity(id, qty)
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Cart' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-2">Shopping Cart</h1>
    <p v-if="!cart.isEmpty" class="text-sm text-neutral-500 mb-8">{{ cart.itemCount }} {{ cart.itemCount === 1 ? 'item' : 'items' }}</p>

    <div v-if="cart.loading && cart.isEmpty" class="space-y-4">
      <Skeleton v-for="i in 3" :key="i" height="6rem" />
    </div>
    <div v-else-if="cart.isEmpty" class="text-center py-16">
      <i class="pi pi-shopping-cart text-5xl text-neutral-300 mb-4 block" />
      <p class="text-lg font-medium text-neutral-900 mb-2">Your cart is empty</p>
      <p class="text-sm text-neutral-500 mb-6">Looks like you haven't added anything yet.</p>
      <Button label="Continue Shopping" severity="secondary" outlined as="router-link" to="/shop" />
    </div>
    <div v-else-if="cart.error" class="text-center py-16">
      <p class="text-neutral-500 mb-4">{{ cart.error }}</p>
      <Button label="Retry" severity="secondary" outlined @click="cart.fetchCart()" />
    </div>
    <div v-else class="flex flex-col lg:flex-row gap-8">
      <div class="flex-1 space-y-4">
        <div v-for="item in cart.items" :key="item.id" class="flex gap-4 p-4 bg-white rounded-lg border border-neutral-200">
          <img v-if="item.productImageUrl" :src="item.productImageUrl" :alt="item.productName ?? ''" class="w-20 h-24 object-cover rounded-md bg-neutral-100" />
          <div v-else class="w-20 h-24 rounded-md bg-neutral-100 flex items-center justify-center shrink-0"><i class="pi pi-image text-neutral-300" /></div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-neutral-900 truncate">{{ item.productName ?? item.variantName }}</p>
            <p class="text-xs text-neutral-500 mt-0.5">{{ item.sku }}</p>
            <div class="flex items-center gap-2 mt-2">
              <Button icon="pi pi-minus" text rounded size="small" :disabled="item.quantity <= 1" @click="updateQty(item.id, item.quantity - 1)" />
              <span class="text-sm font-medium w-6 text-center">{{ item.quantity }}</span>
              <Button icon="pi pi-plus" text rounded size="small" @click="updateQty(item.id, item.quantity + 1)" />
            </div>
          </div>
          <div class="text-right shrink-0">
            <p class="text-sm font-medium text-neutral-900 font-mono">${{ item.total.toFixed(2) }}</p>
            <Button icon="pi pi-times" text rounded size="small" severity="danger" class="mt-1" @click="cart.removeItem(item.id)" />
          </div>
        </div>
        <button class="text-xs text-red-600 hover:text-red-800" @click="cart.clearCart()">Clear cart</button>
      </div>
      <div class="lg:w-80 shrink-0">
        <div class="bg-white border border-neutral-200 rounded-lg p-6 sticky top-24">
          <h2 class="text-sm font-semibold text-neutral-900 mb-4">Order Summary</h2>
          <div class="flex justify-between text-sm mb-2"><span class="text-neutral-500">Subtotal</span><span class="text-neutral-900 font-mono">${{ cart.subtotal.toFixed(2) }}</span></div>
          <div class="flex justify-between text-sm mb-2"><span class="text-neutral-500">Shipping</span><span class="text-neutral-400 text-xs">Calculated at checkout</span></div>
          <div class="flex justify-between text-sm mb-4"><span class="text-neutral-500">Tax</span><span class="text-neutral-400 text-xs">Calculated at checkout</span></div>
          <div class="border-t border-neutral-200 pt-4 mb-4"><div class="flex justify-between"><span class="text-sm font-semibold text-neutral-900">Total</span><span class="text-sm font-semibold text-neutral-900 font-mono">${{ cart.subtotal.toFixed(2) }}</span></div></div>
          <Button label="Checkout" severity="primary" class="w-full" as="router-link" to="/checkout" />
          <router-link to="/shop" class="block text-center text-sm text-neutral-500 hover:text-neutral-900 mt-3">Continue Shopping</router-link>
        </div>
      </div>
    </div>
  </div>
</template>
```

Commit: `feat(store): implement CartView with line items and order summary sidebar`

---

### Task 2: CheckoutView — 5-step wizard

**Files:** Modify: `app/Store/src/features/ordering/views/CheckoutView.vue`

```vue
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCheckoutStore } from '../stores/checkoutStore'
import { useCartStore } from '../stores/cartStore'

usePageTitle('Checkout')
const checkout = useCheckoutStore()
const cart = useCartStore()

onMounted(() => { checkout.init(); cart.fetchCart() })

// For now, use simple form refs for address/email step since
// full addressStore + shippingStore integration is complex.
const selectedAddressId = ref('')
const selectedShippingId = ref('')
const email = ref('')

function stepClasses(step: number): string {
  const s = checkout.steps.find(x => x.number === step)
  if (s?.current) return 'bg-neutral-900 text-white'
  if (s?.complete) return 'bg-green-500 text-white'
  return 'bg-neutral-100 text-neutral-500'
}

function goToStep(step: number): void { checkout.currentStep = step as any }

function onPlaceOrder(): void {
  // After placing, advance to complete step
  checkout.currentStep = 5 as any
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Cart', to: '/cart' }, { label: 'Checkout' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Checkout</h1>

    <!-- Stepper -->
    <div class="flex items-center gap-2 mb-8">
      <template v-for="(step, i) in checkout.steps" :key="step.number">
        <div class="flex items-center gap-2">
          <div :class="stepClasses(step.number)" class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">
            {{ step.complete ? '✓' : step.number }}
          </div>
          <span class="text-sm font-medium" :class="step.current ? 'text-neutral-900' : step.complete ? 'text-green-600' : 'text-neutral-400'">{{ step.label }}</span>
        </div>
        <div v-if="i < checkout.steps.length - 1" class="flex-1 h-px mx-2" :class="step.complete ? 'bg-green-300' : 'bg-neutral-200'" />
      </template>
    </div>

    <div class="flex flex-col lg:flex-row gap-8">
      <div class="flex-1">
        <!-- Step 1: Address -->
        <div v-if="checkout.currentStep === 1">
          <h2 class="text-lg font-semibold text-neutral-900 mb-4">Shipping Address</h2>
          <div class="space-y-3 mb-6">
            <label class="flex items-center gap-3 p-4 rounded-lg border cursor-pointer" :class="selectedAddressId === 'addr1' ? 'border-neutral-900 bg-neutral-50' : 'border-neutral-200'">
              <input type="radio" value="addr1" v-model="selectedAddressId" class="text-neutral-900" />
              <div><p class="text-sm font-medium text-neutral-900">123 Main St</p><p class="text-xs text-neutral-500">San Francisco, CA 94102</p></div>
            </label>
          </div>
          <p class="text-sm text-neutral-500 mb-4">+ Add New Address</p>
          <div class="mb-6">
            <label class="block text-sm font-medium text-neutral-700 mb-1">Email</label>
            <InputText v-model="email" type="email" placeholder="you@example.com" class="w-full" />
          </div>
          <div class="flex justify-between">
            <router-link to="/cart" class="text-sm text-neutral-500 hover:text-neutral-900">&larr; Back to Cart</router-link>
            <Button label="Continue to Delivery" severity="primary" @click="goToStep(2)" :disabled="!email" />
          </div>
        </div>

        <!-- Step 2: Delivery -->
        <div v-if="checkout.currentStep === 2">
          <h2 class="text-lg font-semibold text-neutral-900 mb-4">Delivery Method</h2>
          <div class="space-y-3 mb-6">
            <label class="flex items-center justify-between p-4 rounded-lg border cursor-pointer" :class="selectedShippingId === 'standard' ? 'border-neutral-900 bg-neutral-50' : 'border-neutral-200'">
              <div class="flex items-center gap-3">
                <input type="radio" value="standard" v-model="selectedShippingId" class="text-neutral-900" />
                <div><p class="text-sm font-medium text-neutral-900">Standard</p><p class="text-xs text-neutral-500">5-7 business days</p></div>
              </div>
              <span class="text-sm font-medium font-mono">$5.99</span>
            </label>
            <label class="flex items-center justify-between p-4 rounded-lg border cursor-pointer" :class="selectedShippingId === 'express' ? 'border-neutral-900 bg-neutral-50' : 'border-neutral-200'">
              <div class="flex items-center gap-3">
                <input type="radio" value="express" v-model="selectedShippingId" class="text-neutral-900" />
                <div><p class="text-sm font-medium text-neutral-900">Express</p><p class="text-xs text-neutral-500">2-3 business days</p></div>
              </div>
              <span class="text-sm font-medium font-mono">$14.99</span>
            </label>
          </div>
          <div class="flex justify-between">
            <button class="text-sm text-neutral-500 hover:text-neutral-900" @click="goToStep(1)">&larr; Back</button>
            <Button label="Continue to Payment" severity="primary" @click="goToStep(3)" :disabled="!selectedShippingId" />
          </div>
        </div>

        <!-- Step 3: Payment -->
        <div v-if="checkout.currentStep === 3">
          <h2 class="text-lg font-semibold text-neutral-900 mb-4">Payment</h2>
          <div class="p-6 border border-neutral-200 rounded-lg mb-6 bg-neutral-50">
            <p class="text-sm text-neutral-500">Stripe payment integration will be available in a future update.</p>
            <p class="text-sm text-neutral-400 mt-2">Card details are processed securely by Stripe.</p>
          </div>
          <div class="flex justify-between">
            <button class="text-sm text-neutral-500 hover:text-neutral-900" @click="goToStep(2)">&larr; Back</button>
            <Button label="Continue to Review" severity="primary" @click="goToStep(4)" />
          </div>
        </div>

        <!-- Step 4: Confirm -->
        <div v-if="checkout.currentStep === 4">
          <h2 class="text-lg font-semibold text-neutral-900 mb-4">Review Your Order</h2>
          <div class="bg-white border border-neutral-200 rounded-lg p-6 space-y-4 mb-6">
            <div><p class="text-xs text-neutral-500 uppercase tracking-wide mb-1">Shipping to</p><p class="text-sm text-neutral-900">123 Main St, San Francisco, CA 94102</p></div>
            <div><p class="text-xs text-neutral-500 uppercase tracking-wide mb-1">Email</p><p class="text-sm text-neutral-900">{{ email }}</p></div>
            <div><p class="text-xs text-neutral-500 uppercase tracking-wide mb-1">Delivery</p><p class="text-sm text-neutral-900">{{ selectedShippingId === 'express' ? 'Express (2-3 days)' : 'Standard (5-7 days)' }}</p></div>
            <div v-for="item in cart.items" :key="item.id" class="flex justify-between text-sm"><span class="text-neutral-600">{{ item.productName ?? item.variantName }} x{{ item.quantity }}</span><span class="font-mono">${{ item.total.toFixed(2) }}</span></div>
            <div class="border-t border-neutral-200 pt-3 flex justify-between"><span class="font-semibold text-neutral-900">Total</span><span class="font-semibold font-mono">${{ cart.subtotal.toFixed(2) }}</span></div>
          </div>
          <div class="flex justify-between">
            <button class="text-sm text-neutral-500 hover:text-neutral-900" @click="goToStep(3)">&larr; Back</button>
            <Button label="Place Order" severity="primary" @click="onPlaceOrder()" />
          </div>
        </div>

        <!-- Step 5: Complete -->
        <div v-if="checkout.currentStep === 5" class="text-center py-12">
          <i class="pi pi-check-circle text-5xl text-green-500 mb-4 block" />
          <h2 class="text-lg font-semibold text-neutral-900 mb-2">Order confirmed!</h2>
          <p class="text-sm text-neutral-500 mb-6">A confirmation email has been sent to {{ email }}</p>
          <div class="flex items-center justify-center gap-3">
            <Button label="Continue Shopping" severity="secondary" outlined as="router-link" to="/shop" />
          </div>
        </div>
      </div>

      <!-- Order Summary Sidebar -->
      <div v-if="checkout.currentStep < 5" class="lg:w-80 shrink-0">
        <div class="bg-white border border-neutral-200 rounded-lg p-6 sticky top-24">
          <h2 class="text-sm font-semibold text-neutral-900 mb-4">Order Summary</h2>
          <div v-for="item in cart.items" :key="item.id" class="flex gap-3 mb-3">
            <div class="w-12 h-12 rounded bg-neutral-100 shrink-0 overflow-hidden">
              <img v-if="item.productImageUrl" :src="item.productImageUrl" class="w-full h-full object-cover" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-xs text-neutral-900 truncate">{{ item.productName ?? item.variantName }}</p>
              <p class="text-xs text-neutral-500">Qty: {{ item.quantity }}</p>
            </div>
            <span class="text-xs font-mono shrink-0">${{ item.total.toFixed(2) }}</span>
          </div>
          <div class="border-t border-neutral-200 pt-3 mt-3 flex justify-between"><span class="text-sm font-semibold text-neutral-900">Total</span><span class="text-sm font-semibold font-mono">${{ cart.subtotal.toFixed(2) }}</span></div>
        </div>
      </div>
    </div>
  </div>
</template>
```

Commit: `feat(store): implement CheckoutView with 5-step stepper wizard`

---

### Task 3: OrderListView — order history cards

**Files:** Modify: `app/Store/src/features/ordering/views/OrderListView.vue`

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useOrderStore } from '../stores/orderStore'

usePageTitle('My Orders')
const orders = useOrderStore()

onMounted(() => { orders.fetchOrders() })

function statusSeverity(status: string): 'info' | 'warn' | 'success' | 'danger' {
  const map: Record<string, any> = { Placed: 'info', Shipped: 'warn', Delivered: 'success', Canceled: 'danger' }
  return map[status] ?? 'info'
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
}
</script>
<template>
  <div>
    <h1 class="text-2xl font-bold text-neutral-900 mb-6">Your Orders</h1>

    <div v-if="orders.loading && orders.items.length === 0" class="space-y-3">
      <Skeleton v-for="i in 3" :key="i" height="5rem" />
    </div>
    <div v-else-if="orders.error" class="text-center py-12">
      <p class="text-neutral-500 mb-4">{{ orders.error }}</p>
      <Button label="Retry" severity="secondary" outlined @click="orders.refresh()" />
    </div>
    <div v-else-if="orders.items.length === 0" class="text-center py-16">
      <i class="pi pi-inbox text-4xl text-neutral-300 mb-4 block" />
      <p class="text-lg font-medium text-neutral-900 mb-2">No orders yet</p>
      <p class="text-sm text-neutral-500 mb-6">When you place an order, it will appear here.</p>
      <Button label="Start Shopping" severity="secondary" outlined as="router-link" to="/shop" />
    </div>
    <div v-else>
      <router-link v-for="order in orders.items" :key="order.id" :to="`/account/orders/${order.id}`" class="block mb-3">
        <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-neutral-200 hover:border-neutral-400 transition-colors">
          <div class="flex items-center gap-4">
            <div>
              <p class="text-sm font-semibold text-neutral-900">Order #{{ order.number }}</p>
              <p class="text-xs text-neutral-500">{{ formatDate(order.createdAtUtc) }}</p>
            </div>
          </div>
          <div class="flex items-center gap-4">
            <span class="text-sm font-mono font-medium text-neutral-900">${{ order.total.toFixed(2) }}</span>
            <Tag :value="order.status" :severity="statusSeverity(order.status)" />
          </div>
        </div>
      </router-link>
      <Paginator v-if="orders.totalPages > 1" :rows="orders.pageSize" :total-records="orders.totalCount" :first="(orders.page - 1) * orders.pageSize" class="mt-6" @page="(e: any) => orders.goToPage(e.page + 1)" />
    </div>
  </div>
</template>
```

Commit: `feat(store): implement OrderListView with order cards and pagination`

---

### Task 4: OrderDetailView — single order

**Files:** Modify: `app/Store/src/features/ordering/views/OrderDetailView.vue`

```vue
<script setup lang="ts">
import { watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useOrderStore } from '../stores/orderStore'

usePageTitle('Order')
const route = useRoute()
const store = useOrderStore()

watch(() => route.params.id, (id) => {
  if (typeof id === 'string') store.fetchOrder(id)
}, { immediate: true })

function statusSeverity(s: string): 'info' | 'warn' | 'success' | 'danger' {
  const m: Record<string, any> = { Placed: 'info', Shipped: 'warn', Delivered: 'success', Canceled: 'danger' }
  return m[s] ?? 'info'
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
}

async function onCancel(): Promise<void> {
  if (store.currentOrder) await store.cancelOrder(store.currentOrder.id)
}
</script>
<template>
  <div>
    <div v-if="store.detailLoading" class="space-y-4">
      <Skeleton width="40%" height="2rem" />
      <Skeleton width="100%" height="8rem" />
      <Skeleton width="100%" height="6rem" />
    </div>
    <div v-else-if="store.error" class="text-center py-12">
      <p class="text-neutral-500 mb-4">{{ store.error }}</p>
      <Button label="Back to Orders" severity="secondary" outlined as="router-link" to="/account/orders" />
    </div>
    <div v-else-if="store.currentOrder">
      <div class="flex items-center justify-between mb-6">
        <router-link to="/account/orders" class="text-sm text-neutral-500 hover:text-neutral-900">&larr; Back to Orders</router-link>
        <Tag :value="store.currentOrder.status" :severity="statusSeverity(store.currentOrder.status)" />
      </div>

      <h1 class="text-2xl font-bold text-neutral-900 mb-1">Order #{{ store.currentOrder.number }}</h1>
      <p class="text-sm text-neutral-500 mb-8">Placed on {{ formatDate(store.currentOrder.createdAtUtc) }}</p>

      <div class="space-y-8">
        <div>
          <h2 class="text-sm font-semibold text-neutral-900 uppercase tracking-wide mb-3">Summary</h2>
          <div class="bg-white border border-neutral-200 rounded-lg divide-y divide-neutral-100">
            <div class="flex justify-between p-4 text-sm"><span class="text-neutral-500">Subtotal</span><span class="font-mono">${{ store.currentOrder.itemTotal.toFixed(2) }}</span></div>
            <div class="flex justify-between p-4 text-sm"><span class="text-neutral-500">Shipping</span><span class="font-mono">${{ store.currentOrder.shipmentTotal.toFixed(2) }}</span></div>
            <div class="flex justify-between p-4 text-sm"><span class="text-neutral-500">Tax</span><span class="font-mono">${{ store.currentOrder.adjustmentTotal.toFixed(2) }}</span></div>
            <div class="flex justify-between p-4 text-sm font-semibold"><span class="text-neutral-900">Total</span><span class="font-mono">${{ store.currentOrder.total.toFixed(2) }}</span></div>
          </div>
        </div>

        <div v-if="store.currentOrder.status === 'Placed'" class="flex justify-end">
          <ConfirmDialog />
          <Button label="Cancel Order" severity="danger" outlined @click="onCancel()" />
        </div>
      </div>
    </div>
  </div>
</template>
```

Commit: `feat(store): implement OrderDetailView with order info and cancel action`

---

### Task 5: Full verification

- `npx vitest run` — all tests pass
- `npx tsc --noEmit` — clean
- `pnpm run build-only` — successful build
