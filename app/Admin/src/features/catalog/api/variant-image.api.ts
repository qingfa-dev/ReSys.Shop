import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { VariantImageDetailResponse, VariantImageListResponse, UpdateImageMetadataRequest, EmbeddingRequest, EmbeddingDetailResponse } from '../types'

export class VariantImageApi {
  static async list(variantId: string): Promise<Result<VariantImageListResponse>> {
    const res = await apiClient.get<Result<VariantImageListResponse>>(`/catalog/variants/${variantId}/images`)
    return res.data
  }

  static async get(imageId: string): Promise<Result<VariantImageDetailResponse>> {
    const res = await apiClient.get<Result<VariantImageDetailResponse>>(`/catalog/variants/images/${imageId}`)
    return res.data
  }

  static async upload(variantId: string, formData: FormData): Promise<Result<VariantImageDetailResponse>> {
    const res = await apiClient.post<Result<VariantImageDetailResponse>>(
      `/catalog/variants/${variantId}/images`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    return res.data
  }

  static async update(imageId: string, data: UpdateImageMetadataRequest): Promise<Result<VariantImageDetailResponse>> {
    const res = await apiClient.put<Result<VariantImageDetailResponse>>(`/catalog/variants/images/${imageId}`, data)
    return res.data
  }

  static async delete(imageId: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/variants/images/${imageId}`)
    return res.data
  }

  static async download(imageId: string): Promise<Blob> {
    const res = await apiClient.get<Blob>(`/catalog/variants/images/${imageId}/download`, { responseType: 'blob' })
    return res.data
  }

  static async createEmbedding(imageId: string, data: EmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    const res = await apiClient.post<Result<EmbeddingDetailResponse>>(`/catalog/variants/images/${imageId}/embeddings`, data)
    return res.data
  }

  static async regenerateEmbedding(imageId: string, data: EmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    const res = await apiClient.put<Result<EmbeddingDetailResponse>>(`/catalog/variants/images/${imageId}/embeddings`, data)
    return res.data
  }
}
