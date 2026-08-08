<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCheckoutStore } from '../stores/checkoutStore'
import { useCartStore } from '../stores/cartStore'

usePageTitle('Checkout')
const checkout = useCheckoutStore()
const cart = useCartStore()

onMounted(() => { checkout.init(); cart.fetchCart() })

// Bind: Simple form refs for address/email step placeholder fields.
const selectedAddressId = ref('')
const selectedShippingId = ref('')
const email = ref('')

// Compute: Step circle class based on completion and active state.
function stepClasses(step: number): string {
  const s = checkout.steps.find(x => x.number === step)
  if (s?.current) return 'bg-neutral-900 text-white'
  if (s?.complete) return 'bg-green-500 text-white'
  return 'bg-neutral-100 text-neutral-500'
}

// Navigate: Advance wizard to a specific step on user click.
function goToStep(step: number): void { checkout.currentStep = step as any }

// Action: Complete the order and advance to the final confirmation step.
function onPlaceOrder(): void {
  checkout.currentStep = 5 as any
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation and page title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Cart', to: '/cart' }, { label: 'Checkout' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Checkout</h1>

    <!-- Section: Checkout Stepper — 5-step progress indicator -->
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

      <!-- Section: Order Summary Sidebar — collapsed on mobile, visible desktop -->
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
