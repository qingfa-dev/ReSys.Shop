import { get, post, put, del, getBlob } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { VariantImage, VariantImageUpdateRequest, VariantImageUploadRequest } from '../types/variantImage'

export class VariantImageApi {
  static listImages(variantId: string, params: QueryingParameters = {}): Promise<PagedResult<VariantImage>> {
    return getPaged<VariantImage>(`/api/admin/catalog/variant-images?variantId=${variantId}`, params)
  }

  static getImage(id: string): Promise<Result<VariantImage>> {
    return get<Result<VariantImage>>(`/api/admin/catalog/variant-images/${id}`)
  }

  static uploadImage(request: VariantImageUploadRequest): Promise<Result<VariantImage>> {
    const formData = new FormData()
    formData.append('variantId', request.variantId)
    formData.append('file', request.file)
    if (request.alt !== undefined) formData.append('alt', request.alt)
    if (request.position !== undefined) formData.append('position', String(request.position))
    if (request.type !== undefined) formData.append('type', request.type)
    return post<Result<VariantImage>>('/api/admin/catalog/variant-images', formData)
  }

  static updateImage(
    id: string,
    request: VariantImageUpdateRequest,
  ): Promise<Result<VariantImage>> {
    return put<Result<VariantImage>>(`/api/admin/catalog/variant-images/${id}`, request)
  }

  static deleteImage(id: string): Promise<Result<{ message: string }>> {
    return del<Result<{ message: string }>>(`/api/admin/catalog/variant-images/${id}`)
  }

  static downloadImage(id: string): Promise<Blob> {
    return getBlob(`/api/admin/catalog/variant-images/${id}/download`)
  }
}
