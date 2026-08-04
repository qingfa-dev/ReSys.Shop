import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import { VARIANT_IMAGE_FILTER_FIELDS, VARIANT_IMAGE_SORT_FIELDS, VARIANT_IMAGE_SEARCH_FIELDS } from '../types/variantImage'
import type { VariantImage } from '../types/variantImage'

export function useVariantImageList(variantId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<VariantImage>(() => `${CATALOG}/variant-images?variantId=${variantId}`, {
    allowedFilterFields: VARIANT_IMAGE_FILTER_FIELDS,
    allowedSortFields: VARIANT_IMAGE_SORT_FIELDS,
    allowedSearchFields: VARIANT_IMAGE_SEARCH_FIELDS,
    ...options,
  })
}
