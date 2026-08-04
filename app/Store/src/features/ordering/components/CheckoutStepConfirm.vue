<script setup lang="ts">
import { useCartStore } from '../stores/cartStore'
import { useCheckoutStore } from '../stores/checkoutStore'
import { formatVnd } from '@/shared/utils/currency'

const checkout = useCheckoutStore()
const cart = useCartStore()

async function placeOrder(): Promise<void> {
  await checkout.placeOrder()
}
</script>
<template>
  <div class="bg-white rounded-xl border border-stone-200 p-6">
    <h2 class="text-lg font-semibold text-stone-900 mb-4">Confirm Order</h2>
    <div class="border border-stone-200 rounded-lg divide-y divide-gray-200 mb-6">
      <div class="flex justify-between px-4 py-3 text-sm">
        <span class="text-stone-600">Items</span>
        <span class="text-stone-900 font-medium">{{ cart.itemCount }}</span>
      </div>
      <div class="flex justify-between px-4 py-3 text-sm">
        <span class="text-stone-600">Subtotal</span>
        <span class="text-stone-900 font-medium">{{ formatVnd(cart.subtotal) }}</span>
      </div>
      <div class="flex justify-between px-4 py-3 text-sm">
        <span class="text-stone-600">Shipping</span>
        <span class="text-stone-400">Calculated at delivery</span>
      </div>
      <div class="flex justify-between px-4 py-3 text-sm">
        <span class="text-stone-600">Contact email</span>
        <span class="text-stone-900 font-medium">{{ checkout.email }}</span>
      </div>
    </div>
    <div class="flex justify-between">
      <Button label="Back" icon="pi pi-arrow-left" severity="secondary" :disabled="checkout.loading" @click="checkout.goToStep(3)" />
      <Button label="Place Order" icon="pi pi-check" :loading="checkout.loading" :disabled="cart.items.length === 0" @click="placeOrder" />
    </div>
  </div>
</template>
