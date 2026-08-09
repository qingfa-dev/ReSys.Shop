import { ref, computed, reactive } from 'vue'
import { AddressApi } from '../services/addressApi'
import type { Address, AddressInput } from '../types'

// Module-level singleton state
const addresses = ref<Address[]>([])
const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)

// Compute: Derive the single default address from the full list
const defaultAddress = computed(() => addresses.value.find(a => a.isDefault))
// Filter: Include only shipping-eligible types (Shipping or Other)
const shippingAddresses = computed(() =>
  addresses.value.filter(a => a.addressType === 'Shipping' || a.addressType === 'Other'),
)

async function fetchAddresses(): Promise<void> {
  loading.value = true
  const result = await AddressApi.getAddresses()
  if (result.isSuccess) addresses.value = result.items
  else error.value = result.message
  loading.value = false
}

async function createAddress(req: AddressInput): Promise<boolean> {
  saving.value = true
  const result = await AddressApi.createAddress(req)
  if (result.isSuccess) addresses.value.push(result.value)
  else error.value = result.message
  saving.value = false
  return result.isSuccess
}

// Update: Replace address in-place by matching id
async function updateAddress(id: string, req: AddressInput): Promise<boolean> {
  saving.value = true
  const result = await AddressApi.updateAddress(id, req)
  if (result.isSuccess) {
    const idx = addresses.value.findIndex(a => a.id === id)
    if (idx !== -1) addresses.value[idx] = result.value
  } else {
    error.value = result.message
  }
  saving.value = false
  return result.isSuccess
}

// Remove: Filter out deleted address by id
async function deleteAddress(id: string): Promise<boolean> {
  saving.value = true
  const result = await AddressApi.deleteAddress(id)
  if (result.isSuccess) addresses.value = addresses.value.filter(a => a.id !== id)
  else error.value = result.message
  saving.value = false
  return result.isSuccess
}

export function useAddresses() {
  return reactive({
    addresses,
    loading,
    saving,
    error,
    defaultAddress,
    shippingAddresses,
    fetchAddresses,
    createAddress,
    updateAddress,
    deleteAddress,
  })
}
