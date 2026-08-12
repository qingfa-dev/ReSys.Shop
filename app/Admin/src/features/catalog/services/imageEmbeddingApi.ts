import { get, post, put, del } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type {
  CreateEmbeddingRequest,
  RegenerateEmbeddingRequest,
  EmbeddingDetailResponse,
} from '../types/imageEmbedding'

export class ImageEmbeddingApi {
  static create(request: CreateEmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    return post<Result<EmbeddingDetailResponse>>('/api/admin/catalog/variant-image-embeddings', request)
  }

  static regenerate(request: RegenerateEmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    return put<Result<EmbeddingDetailResponse>>('/api/admin/catalog/variant-image-embeddings/regenerate', request)
  }

  static get(variantImageId: string): Promise<Result<EmbeddingDetailResponse>> {
    return get<Result<EmbeddingDetailResponse>>(`/api/admin/catalog/variant-image-embeddings/${variantImageId}`)
  }

  static deleteEmbedding(variantImageId: string): Promise<Result<{ message: string }>> {
    return del<Result<{ message: string }>>(`/api/admin/catalog/variant-image-embeddings/${variantImageId}`)
  }
}
