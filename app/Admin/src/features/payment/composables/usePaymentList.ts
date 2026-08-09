import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { PAYMENT_FILTER_FIELDS, PAYMENT_SORT_FIELDS, PAYMENT_SEARCH_FIELDS } from '../types/payment'
import type { PaymentListItem } from '../types/payment'

export function usePaymentList(options?: UsePagedQueryOptions) {
  return usePagedQuery<PaymentListItem>('api/admin/billing/payments', {
    allowedFilterFields: PAYMENT_FILTER_FIELDS,
    allowedSortFields: PAYMENT_SORT_FIELDS,
    allowedSearchFields: PAYMENT_SEARCH_FIELDS,
    ...options,
  })
}
