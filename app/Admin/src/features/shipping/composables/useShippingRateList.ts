import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { SHIPPING } from '@/shared/constants/api'
import { SHIPPING_RATE_FILTER_FIELDS, SHIPPING_RATE_SORT_FIELDS, SHIPPING_RATE_SEARCH_FIELDS } from '../types/shippingRate'
import type { ShippingRateListItem } from '../types/shippingRate'

export function useShippingRateList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ShippingRateListItem>(`${SHIPPING}/shipping-rates`, {
    allowedFilterFields: SHIPPING_RATE_FILTER_FIELDS,
    allowedSortFields: SHIPPING_RATE_SORT_FIELDS,
    allowedSearchFields: SHIPPING_RATE_SEARCH_FIELDS,
    ...options,
  })
}
