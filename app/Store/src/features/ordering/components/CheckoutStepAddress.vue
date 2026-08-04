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

// Trigger: Fetch the user's real address book on mount.
onMounted(async () => {
  const result = await getAddresses()
  if (result.isSuccess) {
    addresses.value = result.items
    const preferred = result.items.find((addr) => addr.isDefault) ?? result.items[0]
    if (preferred) selectedAddressId.value = preferred.id
  } else {
    localError.value = result.message ?? 'Failed to load addresses'
  }
  loading.value = false
})

// Map: Human-readable address display line.
function addressSummary(addr: Address): string {
  const parts = [
    addr.address1,
    addr.address2,
    addr.city,
    addr.stateProvince,
    addr.countryName,
  ].filter((part): part is string => !!part)
  return parts.join(', ')
}

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
          <span class="font-medium text-stone-900">
            {{ addr.label ?? [addr.firstName, addr.lastName].filter(Boolean).join(' ') }}
          </span>
          <span v-if="addr.isDefault" class="ml-2 text-teal-600">(Default)</span>
          <span class="block text-stone-500">{{ addressSummary(addr) }}</span>
        </label>
      </div>
    </div>

    <!-- Section: Actions -->
    <div class="flex justify-end">
      <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" :disabled="!selectedAddressId || checkout.loading" @click="continueToDelivery" />
    </div>
  </div>
</template>
