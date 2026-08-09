import { get, post, put, del } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type {
  CreateEmbeddingRequest,
  RegenerateEmbeddingRequest,
  EmbeddingDetailResponse,
} from '../types/imageEmbedding'

export class ImageEmbeddingApi {
  private static readonly BASE = 'api/admin/catalog/variant-image-embeddings'

  static create(request: CreateEmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    return post<Result<EmbeddingDetailResponse>>(ImageEmbeddingApi.BASE, request)
  }

  static regenerate(request: RegenerateEmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    return put<Result<EmbeddingDetailResponse>>(`${ImageEmbeddingApi.BASE}/regenerate`, request)
  }

  static get(variantImageId: string): Promise<Result<EmbeddingDetailResponse>> {
    return get<Result<EmbeddingDetailResponse>>(`${ImageEmbeddingApi.BASE}/${variantImageId}`)
  }

  static deleteEmbedding(variantImageId: string): Promise<Result<{ message: string }>> {
    return del<Result<{ message: string }>>(`${ImageEmbeddingApi.BASE}/${variantImageId}`)
  }
}
