import { ref, reactive } from 'vue'
import { getShippingMethods, getShippingRates, calculateShipping } from '../services/shippingApi'
import type { ShippingCost, ShippingMethod, ShippingRate } from '../types/shipping'

// Module-level singleton state
const methods = ref<ShippingMethod[]>([])
const rates = ref<ShippingRate[]>([])
const selectedMethodId = ref<string | null>(null)
const preview = ref<ShippingCost | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const _initialized = ref(false)

// Fetch: Load available shipping methods once on first access.
async function fetchMethods(): Promise<void> {
  if (_initialized.value) return
  _initialized.value = true
  loading.value = true
  error.value = null
  const result = await getShippingMethods()
  if (result.isSuccess) {
    methods.value = result.items
  } else {
    error.value = result.message
  }
  loading.value = false
}

// Fetch: Load the available shipping rates for selection.
async function fetchRates(): Promise<void> {
  loading.value = true
  error.value = null
  const result = await getShippingRates()
  if (result.isSuccess) {
    rates.value = result.items
  } else {
    error.value = result.message
  }
  loading.value = false
}

// Fetch: Authoritative server-calculated shipping cost preview for a method against the current order.
async function previewFor(methodId: string, orderId: string): Promise<void> {
  preview.value = null
  loading.value = true
  error.value = null
  const result = await calculateShipping(methodId, orderId)
  if (result.isSuccess) {
    preview.value = result.value
  } else {
    error.value = result.message
  }
  loading.value = false
}

// Assign: Track the user-selected shipping method for checkout.
function selectMethod(id: string): void {
  selectedMethodId.value = id
}

export function useShipping() {
  return reactive({
    methods,
    rates,
    selectedMethodId,
    preview,
    loading,
    error,
    fetchMethods,
    fetchRates,
    previewFor,
    selectMethod,
  })
}
