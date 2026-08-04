<script setup lang="ts">
import { ref } from 'vue'
import { useCheckoutStore } from '../stores/checkoutStore'

const checkout = useCheckoutStore()

// Placeholder shipping methods — the real list arrives with the Shipping module (Phase 5).
const shippingOptions = [
  { id: 'b2c4f1d0-3e4a-4b5c-9d8e-7f6a5b4c3d2e', label: 'Standard — 3-5 business days', price: 30000 },
  { id: 'c3d5a2e1-4f5b-4c6d-9e8f-8a7b6c5d4e3f', label: 'Express — 1-2 business days', price: 60000 },
]

const selectedMethodId = ref<string | null>(null)

async function continueToPayment(): Promise<void> {
  if (!selectedMethodId.value) return
  const calculated = await checkout.calculateShipping(selectedMethodId.value)
  if (calculated) await checkout.goToStep(3)
}
</script>
<template>
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">Delivery Method</h2>
    <div class="mb-6">
      <span class="block text-sm font-medium text-gray-700 mb-2">Shipping options</span>
      <div class="space-y-2">
        <div
          v-for="opt in shippingOptions"
          :key="opt.id"
          class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer"
          :class="selectedMethodId === opt.id ? 'border-gray-900 bg-gray-50' : 'border-gray-200'"
          @click="selectedMethodId = opt.id"
        >
          <RadioButton v-model="selectedMethodId" :input-id="opt.id" :value="opt.id" />
          <label :for="opt.id" class="flex-1 text-sm text-gray-700 cursor-pointer">{{ opt.label }}</label>
        </div>
      </div>
    </div>
    <div class="flex justify-between">
      <Button label="Back" icon="pi pi-arrow-left" severity="secondary" :disabled="checkout.loading" @click="checkout.goToStep(1)" />
      <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" :disabled="!selectedMethodId || checkout.loading" @click="continueToPayment" />
    </div>
  </div>
</template>
