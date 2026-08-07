export interface SearchByImageResponse {
  variantId: string
  productId: string
  productName: string
  sku: string
  price: number
  imageUrl: string | null
  similarityScore: number
}

export interface VisualSearchModel {
  id: string
  name: string
  description: string | null
  dimension: number
  isOnnx: boolean
}
