import { post, put } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type {
  CreateEmbeddingRequest,
  RegenerateEmbeddingRequest,
  EmbeddingDetailResponse,
} from '../types/imageEmbedding'

export class ImageEmbeddingApi {
  private static readonly BASE = `${CATALOG}/variant-image-embeddings`

  static create(request: CreateEmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    return post<Result<EmbeddingDetailResponse>>(ImageEmbeddingApi.BASE, request)
  }

  static regenerate(request: RegenerateEmbeddingRequest): Promise<Result<EmbeddingDetailResponse>> {
    return put<Result<EmbeddingDetailResponse>>(`${ImageEmbeddingApi.BASE}/regenerate`, request)
  }
}
