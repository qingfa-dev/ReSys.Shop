import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { PaymentMethodApi } from '../services/paymentMethodApi'
import type { PaymentMethodListItem } from '../types/paymentMethod'

export function usePaymentMethodList(options?: UsePagedQueryOptions) {
  return usePagedQuery<PaymentMethodListItem>((params) => PaymentMethodApi.getPaymentMethods(params), {
    ...options,
  })
}