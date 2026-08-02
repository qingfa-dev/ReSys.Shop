import { get, post, put, del, getBlob } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type { VariantImage, VariantImageUpdateRequest, VariantImageUploadRequest } from '../types/variantImage'

const BASE = `${CATALOG}/variant-images`

export class VariantImageApi {
  static listImages(variantId: string): Promise<PagedResult<VariantImage>> {
    return getPaged<VariantImage>(`${BASE}?variantId=${variantId}`, {})
  }

  static getImage(id: string): Promise<Result<VariantImage>> {
    return get<Result<VariantImage>>(`${BASE}/${id}`)
  }

  static uploadImage(request: VariantImageUploadRequest): Promise<Result<VariantImage>> {
    const formData = new FormData()
    formData.append('variantId', request.variantId)
    formData.append('file', request.file)
    if (request.alt !== undefined) formData.append('alt', request.alt)
    if (request.position !== undefined) formData.append('position', String(request.position))
    if (request.type !== undefined) formData.append('type', request.type)
    return post<Result<VariantImage>>(BASE, formData)
  }

  static updateImage(
    id: string,
    request: VariantImageUpdateRequest,
  ): Promise<Result<VariantImage>> {
    return put<Result<VariantImage>>(`${BASE}/${id}`, request)
  }

  static deleteImage(id: string): Promise<Result<{ message: string }>> {
    return del<Result<{ message: string }>>(`${BASE}/${id}`)
  }

  static downloadImage(id: string): Promise<Blob> {
    return getBlob(`${BASE}/${id}/download`)
  }
}
