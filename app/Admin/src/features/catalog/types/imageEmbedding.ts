export interface CreateEmbeddingRequest {
  variantImageId?: string
  modelName?: string
}

export interface RegenerateEmbeddingRequest {
  variantImageId?: string
  modelName?: string
  modelVersion?: string
}

export interface EmbeddingDetailResponse {
  id: string
  variantImageId: string
  modelName: string
  modelVersion: string
  vector: number[]
  dimensions: number
  createdAtUtc: string
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
  error?: string
  hangfireJobId?: string
  completedAtUtc?: string
}
