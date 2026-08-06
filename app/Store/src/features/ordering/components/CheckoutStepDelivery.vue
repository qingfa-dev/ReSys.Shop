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

// Trigger: Fetch real shipping methods on mount.
onMounted(async () => {
  const result = await getShippingMethods()
  if (result.isSuccess) {
    methods.value = result.items
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
    <p v-else-if="methods.length === 0" class="text-sm text-stone-500 mb-6">No shipping methods available.</p>

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
          <span v-if="method.adminName || method.code" class="block text-stone-500">
            {{ method.adminName ?? method.code }}
          </span>
          <p v-if="(method as any).deliveryRange" class="text-xs text-stone-500">
            Est. delivery: {{ (method as any).deliveryRange }}
          </p>
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
