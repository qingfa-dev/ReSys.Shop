import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { ShippingRateApi } from '../services/shippingRateApi'
import type { ShippingRateListItem } from '../types/shippingRate'

export function useShippingRateList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ShippingRateListItem>((params) => ShippingRateApi.getShippingRates(params), {
    ...options,
  })
}