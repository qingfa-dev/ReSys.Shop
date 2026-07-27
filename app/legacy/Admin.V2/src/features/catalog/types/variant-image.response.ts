export interface VariantImageDetailResponse {
  id: string
  variantId?: string | null
  alt?: string | null
  position: number
  type: string
  url: string
  contentType: string
  fileName: string
  fileSize: number
  width?: number | null
  height?: number | null
  dimensionsUnit?: string | null
  createdAt: string
}

export interface VariantImageListResponse {
  images: VariantImageDetailResponse[]
}

export interface EmbeddingDetailResponse {
  id: string
  variantImageId: string
  modelName: string
  modelVersion: string
  dimensions: number
  createdAt: string
}
