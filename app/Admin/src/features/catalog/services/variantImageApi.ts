import { post, get, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type { VariantImage } from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantImageApi {
  static listImages(variantId: string): Promise<PagedResult<VariantImage>> {
    return getPaged<VariantImage>(`${BASE}/${variantId}/images`, {
      pageNumber: 1,
      pageSize: 100,
    })
  }

  static uploadImage(
    variantId: string,
    file: File,
  ): Promise<Result<VariantImage>> {
    const formData = new FormData()
    formData.append('file', file)
    return post<Result<VariantImage>>(`${BASE}/${variantId}/images`, formData)
  }

  static deleteImage(imageId: string): Promise<Result<{ message: string }>> {
    return del<Result<{ message: string }>>(`${CATALOG}/variants/images/${imageId}`)
  }
}
