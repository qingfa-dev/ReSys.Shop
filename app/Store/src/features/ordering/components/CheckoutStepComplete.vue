<script setup lang="ts">
import { useCheckoutStore } from '../stores/checkoutStore'

const checkout = useCheckoutStore()

function continueShopping(): void {
  checkout.reset()
}
</script>
<template>
  <div class="text-center bg-white rounded-xl border border-stone-200 p-10">
    <i class="pi pi-check-circle text-5xl text-green-500 mb-4 block" />
    <h2 class="text-xl font-semibold text-stone-900 mb-2">Order Confirmed</h2>
    <p class="text-stone-600 mb-1">Thank you! Your order has been placed successfully.</p>
    <p v-if="checkout.orderId" class="text-sm text-stone-500 mb-6">Order ID: {{ checkout.orderId }}</p>
    <div class="flex justify-center gap-3">
      <router-link to="/shop">
        <Button label="Continue Shopping" @click="continueShopping" />
      </router-link>
      <router-link to="/account/orders">
        <Button label="View Orders" severity="secondary" />
      </router-link>
    </div>
  </div>
</template>
