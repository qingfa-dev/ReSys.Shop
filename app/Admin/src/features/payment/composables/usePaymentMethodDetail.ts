import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { PaymentMethodDetail } from '../types/paymentMethod'
import { PaymentMethodApi } from '../services/paymentMethodApi'

export interface UsePaymentMethodDetailState {
  paymentMethod: Ref<PaymentMethodDetail | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchPaymentMethod: (id: string) => Promise<Result<PaymentMethodDetail>>
}

export function usePaymentMethodDetail() {
  const paymentMethod = ref<PaymentMethodDetail | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPaymentMethod(id: string): Promise<Result<PaymentMethodDetail>> {
    loading.value = true
    error.value = null
    const result = await PaymentMethodApi.getPaymentMethod(id)
    loading.value = false
    if (result.isSuccess) {
      paymentMethod.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { paymentMethod, loading, error, fetchPaymentMethod }
}
