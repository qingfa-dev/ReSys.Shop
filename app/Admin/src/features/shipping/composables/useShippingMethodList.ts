import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { ShippingMethodApi } from '../services/shippingMethodApi'
import type { ShippingMethodListItem } from '../types/shippingMethod'

export function useShippingMethodList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ShippingMethodListItem>((params) => ShippingMethodApi.getShippingMethods(params), {
    ...options,
  })
}