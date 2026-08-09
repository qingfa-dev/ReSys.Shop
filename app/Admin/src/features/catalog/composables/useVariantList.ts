import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { VARIANT_FILTER_FIELDS, VARIANT_SORT_FIELDS, VARIANT_SEARCH_FIELDS } from '../types/variant'
import type { VariantListItem } from '../types/variant'

export function useVariantList(productId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<VariantListItem>(() => `api/admin/catalog/variants?productId=${productId}`, {
    allowedFilterFields: VARIANT_FILTER_FIELDS,
    allowedSortFields: VARIANT_SORT_FIELDS,
    allowedSearchFields: VARIANT_SEARCH_FIELDS,
    ...options,
  })
}
