import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { ShippingMethodDetail } from '../types/shippingMethod'
import { ShippingMethodApi } from '../services/shippingMethodApi'

export interface UseShippingMethodDetailState {
  shippingMethod: Ref<ShippingMethodDetail | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchShippingMethod: (id: string) => Promise<Result<ShippingMethodDetail>>
}

export function useShippingMethodDetail() {
  const shippingMethod = ref<ShippingMethodDetail | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchShippingMethod(id: string): Promise<Result<ShippingMethodDetail>> {
    loading.value = true
    error.value = null
    const result = await ShippingMethodApi.getShippingMethod(id)
    loading.value = false
    if (result.isSuccess) {
      shippingMethod.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { shippingMethod, loading, error, fetchShippingMethod }
}
