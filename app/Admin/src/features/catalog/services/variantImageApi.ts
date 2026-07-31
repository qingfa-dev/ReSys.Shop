import { post, get, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { VariantImage } from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantImageApi {
  static listImages(
    variantId: string,
  ): Promise<Result<{ images: VariantImage[] }>> {
    return get<Result<{ images: VariantImage[] }>>(
      `${BASE}/${variantId}/images`,
    )
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
