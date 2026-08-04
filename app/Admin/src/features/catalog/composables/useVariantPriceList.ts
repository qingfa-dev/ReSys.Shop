import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import { VARIANT_PRICE_FILTER_FIELDS, VARIANT_PRICE_SORT_FIELDS, VARIANT_PRICE_SEARCH_FIELDS } from '../types/variantPrice'
import type { Price } from '../types/variantPrice'

export function useVariantPriceList(variantId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<Price>(() => `${CATALOG}/variant-prices?variantId=${variantId}`, {
    allowedFilterFields: VARIANT_PRICE_FILTER_FIELDS,
    allowedSortFields: VARIANT_PRICE_SORT_FIELDS,
    allowedSearchFields: VARIANT_PRICE_SEARCH_FIELDS,
    ...options,
  })
}
