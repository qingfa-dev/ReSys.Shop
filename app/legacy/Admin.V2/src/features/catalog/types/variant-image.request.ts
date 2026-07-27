export interface UploadImageRequest {
  file: File
  alt?: string | null
  position: number
  type: string
}

export interface UpdateImageMetadataRequest {
  alt?: string | null
  position: number
  type: string
}

export interface EmbeddingRequest {
  modelName: string
  modelVersion: string
}

export interface ReorderImagesRequest {
  imageIds: string[]
}
