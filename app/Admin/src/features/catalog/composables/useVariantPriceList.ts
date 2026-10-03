import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { VariantPriceApi } from '../services/variantPriceApi'
import type { Price } from '../types/variantPrice'

export function useVariantPriceList(variantId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<Price>((params) => VariantPriceApi.listPrices(variantId, params), {
    ...options,
  })
}
