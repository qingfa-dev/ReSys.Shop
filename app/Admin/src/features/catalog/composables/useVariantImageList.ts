import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { VariantImageApi } from '../services/variantImageApi'
import type { VariantImage } from '../types/variantImage'

export function useVariantImageList(variantId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<VariantImage>((params) => VariantImageApi.listImages(variantId, params), {
    ...options,
  })
}
