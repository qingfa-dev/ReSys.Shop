<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCheckoutStore } from '../stores/checkoutStore'

const checkout = useCheckoutStore()
const auth = useAuthStore()

// Placeholder addresses — the real address book arrives with the Profile module (Phase 6).
// Ids are GUID-shaped so the backend request binding accepts them.
const addressOptions = [
  { id: 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', label: 'Home — 12 Nguyen Hue, District 1, HCMC' },
  { id: 'f47ac10b-58cc-4372-a567-0e02b2c3d479', label: 'Office — 88 Le Loi, District 1, HCMC' },
]

const selectedAddressId = ref<string | null>(null)
const email = ref(auth.user?.email ?? '')

async function continueToDelivery(): Promise<void> {
  if (!selectedAddressId.value) return
  const saved = await checkout.saveAddress(selectedAddressId.value, email.value)
  if (saved) await checkout.goToStep(2)
}
</script>
<template>
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">Shipping Address</h2>
    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700 mb-1" for="checkout-email">Email</label>
      <InputText id="checkout-email" v-model="email" type="email" class="w-full" />
    </div>
    <div class="mb-6">
      <span class="block text-sm font-medium text-gray-700 mb-2">Saved addresses</span>
      <div class="space-y-2">
        <div
          v-for="opt in addressOptions"
          :key="opt.id"
          class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer"
          :class="selectedAddressId === opt.id ? 'border-gray-900 bg-gray-50' : 'border-gray-200'"
          @click="selectedAddressId = opt.id"
        >
          <RadioButton v-model="selectedAddressId" :input-id="opt.id" :value="opt.id" />
          <label :for="opt.id" class="text-sm text-gray-700 cursor-pointer">{{ opt.label }}</label>
        </div>
      </div>
    </div>
    <div class="flex justify-end">
      <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" :disabled="!selectedAddressId || checkout.loading" @click="continueToDelivery" />
    </div>
  </div>
</template>
