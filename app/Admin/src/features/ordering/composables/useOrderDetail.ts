import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { OrderDetail } from '../types/order'
import { OrderApi } from '../services/orderApi'

export interface UseOrderDetailState {
  order: Ref<OrderDetail | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchOrder: (id: string) => Promise<Result<OrderDetail>>
}

export function useOrderDetail() {
  const order = ref<OrderDetail | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchOrder(id: string): Promise<Result<OrderDetail>> {
    loading.value = true
    error.value = null
    const result = await OrderApi.getOrder(id)
    loading.value = false
    if (result.isSuccess) {
      order.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { order, loading, error, fetchOrder }
}
