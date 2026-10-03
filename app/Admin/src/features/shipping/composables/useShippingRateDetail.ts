import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { ShippingRateDetail } from '../types/shippingRate'
import { ShippingRateApi } from '../services/shippingRateApi'

export interface UseShippingRateDetailState {
  shippingRate: Ref<ShippingRateDetail | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchShippingRate: (id: string) => Promise<Result<ShippingRateDetail>>
}

export function useShippingRateDetail() {
  const shippingRate = ref<ShippingRateDetail | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchShippingRate(id: string): Promise<Result<ShippingRateDetail>> {
    loading.value = true
    error.value = null
    const result = await ShippingRateApi.getShippingRate(id)
    loading.value = false
    if (result.isSuccess) {
      shippingRate.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { shippingRate, loading, error, fetchShippingRate }
}
