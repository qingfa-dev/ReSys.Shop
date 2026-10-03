import { z } from 'zod'

export const imageEmbeddingVariantImageId = z.string()
  .optional()

export const imageEmbeddingModelName = z.string()
  .optional()

export const imageEmbeddingModelVersion = z.string()
  .optional()

export const createEmbeddingSchema = z.object({
  variantImageId: imageEmbeddingVariantImageId,
  modelName: imageEmbeddingModelName,
})

export const regenerateEmbeddingSchema = z.object({
  variantImageId: imageEmbeddingVariantImageId,
  modelName: imageEmbeddingModelName,
  modelVersion: imageEmbeddingModelVersion,
})

export type CreateEmbeddingForm = z.infer<typeof createEmbeddingSchema>

export type RegenerateEmbeddingForm = z.infer<typeof regenerateEmbeddingSchema>
