import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { VariantApi } from '../services/variantApi'
import type { VariantListItem } from '../types/variant'

export function useVariantList(productId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<VariantListItem>((params) => VariantApi.getVariants(productId, params), {
    ...options,
  })
}
