import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { ProductApi } from '../services/productApi'
import type { ProductListItem } from '../types/product'

export function useProductList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ProductListItem>((params) => ProductApi.getProducts(params), {
    ...options,
  })
}
