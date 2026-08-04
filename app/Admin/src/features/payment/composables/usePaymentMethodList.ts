import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { PAYMENT } from '@/shared/constants/api'
import {
  PAYMENT_METHOD_FILTER_FIELDS,
  PAYMENT_METHOD_SORT_FIELDS,
  PAYMENT_METHOD_SEARCH_FIELDS,
} from '../types/paymentMethod'
import type { PaymentMethodListItem } from '../types/paymentMethod'

export function usePaymentMethodList(options?: UsePagedQueryOptions) {
  return usePagedQuery<PaymentMethodListItem>(`${PAYMENT}/payment-methods`, {
    allowedFilterFields: PAYMENT_METHOD_FILTER_FIELDS,
    allowedSortFields: PAYMENT_METHOD_SORT_FIELDS,
    allowedSearchFields: PAYMENT_METHOD_SEARCH_FIELDS,
    ...options,
  })
}
