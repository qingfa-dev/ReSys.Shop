import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { PRODUCT_FILTER_FIELDS, PRODUCT_SORT_FIELDS } from '../types/product'
import type { ProductListItem } from '../types/product'

export function useProductList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ProductListItem>(`api/admin/catalog/products`, {
    allowedFilterFields: PRODUCT_FILTER_FIELDS,
    allowedSortFields: PRODUCT_SORT_FIELDS,
    ...options,
  })
}
