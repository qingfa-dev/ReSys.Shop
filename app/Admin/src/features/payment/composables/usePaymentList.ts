import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { PaymentApi } from '../services/paymentApi'
import type { PaymentListItem } from '../types/payment'

export function usePaymentList(options?: UsePagedQueryOptions) {
  return usePagedQuery<PaymentListItem>((params) => PaymentApi.getPayments(params), {
    ...options,
  })
}