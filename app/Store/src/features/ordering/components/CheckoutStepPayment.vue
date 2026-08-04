<script setup lang="ts">
import { ref } from 'vue'
import { useCartStore } from '../stores/cartStore'
import { useCheckoutStore } from '../stores/checkoutStore'
import { formatVnd } from '@/shared/utils/currency'

const checkout = useCheckoutStore()
const cart = useCartStore()

// Placeholder payment methods — the real list + Stripe Elements arrive with the Payment module (Phase 5).
const methodOptions = [
  { id: 'd4e6b3f2-5a6c-4d7e-8f0a-9b8c7d6e5f4a', label: 'Credit / Debit Card (Stripe)' },
  { id: 'e5f7c4a3-6b7d-4e8f-9a1b-ac9d8e7f6a5b', label: 'Cash on Delivery' },
]

const selectedMethodId = ref<string | null>(null)
const processing = ref(false)
const localError = ref<string | null>(null)

async function pay(): Promise<void> {
  if (!selectedMethodId.value || !cart.id) return
  processing.value = true
  localError.value = null
  const clientSecret = await checkout.createPaymentIntent(selectedMethodId.value, cart.subtotal)
  processing.value = false
  if (clientSecret) {
    await checkout.goToStep(4)
  } else {
    localError.value = checkout.error ?? 'Unable to create a payment intent.'
  }
}
</script>
<template>
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">Payment</h2>
    <div class="flex justify-between text-sm text-gray-600 mb-4">
      <span>Order total</span>
      <span class="font-semibold text-gray-900">{{ formatVnd(cart.subtotal) }}</span>
    </div>
    <div class="mb-6">
      <span class="block text-sm font-medium text-gray-700 mb-2">Payment method</span>
      <div class="space-y-2">
        <div
          v-for="opt in methodOptions"
          :key="opt.id"
          class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer"
          :class="selectedMethodId === opt.id ? 'border-gray-900 bg-gray-50' : 'border-gray-200'"
          @click="selectedMethodId = opt.id"
        >
          <RadioButton v-model="selectedMethodId" :input-id="opt.id" :value="opt.id" />
          <label :for="opt.id" class="text-sm text-gray-700 cursor-pointer">{{ opt.label }}</label>
        </div>
      </div>
    </div>
    <Message v-if="localError" severity="error" class="mb-4">{{ localError }}</Message>
    <div class="flex justify-between">
      <Button label="Back" icon="pi pi-arrow-left" severity="secondary" :disabled="processing || checkout.loading" @click="checkout.goToStep(2)" />
      <Button label="Pay" icon="pi pi-credit-card" iconPos="right" :loading="processing" :disabled="!selectedMethodId || !cart.id" @click="pay" />
    </div>
  </div>
</template>
