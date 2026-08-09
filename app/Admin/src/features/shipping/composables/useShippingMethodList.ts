import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { SHIPPING_METHOD_FILTER_FIELDS, SHIPPING_METHOD_SORT_FIELDS, SHIPPING_METHOD_SEARCH_FIELDS } from '../types/shippingMethod'
import type { ShippingMethodListItem } from '../types/shippingMethod'

export function useShippingMethodList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ShippingMethodListItem>(`api/admin/shipping/shipping-methods`, {
    allowedFilterFields: SHIPPING_METHOD_FILTER_FIELDS,
    allowedSortFields: SHIPPING_METHOD_SORT_FIELDS,
    allowedSearchFields: SHIPPING_METHOD_SEARCH_FIELDS,
    ...options,
  })
}
