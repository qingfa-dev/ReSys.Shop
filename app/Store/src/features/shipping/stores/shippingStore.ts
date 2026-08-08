import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getShippingMethods, getShippingRates } from '../services/shippingApi'
import type { ShippingMethod, ShippingRate } from '../types/shipping'
import type { QueryingParameters } from '@/shared/types/querying'

export const useShippingStore = defineStore('shipping', () => {
  const methods = ref<ShippingMethod[]>([])
  const rates = ref<ShippingRate[]>([])
  const selectedMethodId = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const _initialized = ref(false)

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

  async function fetchRates(orderId: string): Promise<void> {
    loading.value = true
    error.value = null
    const params: QueryingParameters = { filter: `orderId eq '${orderId}'` }
    const result = await getShippingRates(params)
    if (result.isSuccess) {
      rates.value = result.items
    } else {
      error.value = result.message
    }
    loading.value = false
  }

  function selectMethod(id: string): void {
    selectedMethodId.value = id
  }

  return { methods, rates, selectedMethodId, loading, error, fetchMethods, fetchRates, selectMethod }
})
