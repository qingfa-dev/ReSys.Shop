import type { StoreProductListItemResponse } from './product'

export type SearchByImageResponse = StoreProductListItemResponse & {
  similarityScore: number
}

export interface VisualSearchModel {
  id: string
  name: string
  description: string | null
  dimension: number
  isOnnx: boolean
}
