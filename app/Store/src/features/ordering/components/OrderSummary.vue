<script setup lang="ts">
import { formatVnd } from '@/shared/utils/currency'

defineProps<{ itemCount: number; subtotal: number }>()
</script>
<template>
  <div class="bg-white rounded-xl border border-stone-200 p-6 sticky top-24">
    <h3 class="text-lg font-semibold text-stone-900 mb-4">Order Summary</h3>
    <div class="flex justify-between text-sm text-stone-600 mb-2">
      <span>Items ({{ itemCount }})</span>
      <span>{{ formatVnd(subtotal) }}</span>
    </div>
    <div class="flex justify-between text-sm text-stone-600 mb-2">
      <span>Shipping</span>
      <span class="text-stone-400">Calculated at checkout</span>
    </div>
    <Divider />
    <div class="flex justify-between font-semibold text-stone-900 mb-6">
      <span>Total</span>
      <span>{{ formatVnd(subtotal) }}</span>
    </div>
    <router-link to="/checkout">
      <Button label="Proceed to Checkout" class="w-full" :disabled="itemCount === 0" />
    </router-link>
  </div>
</template>
