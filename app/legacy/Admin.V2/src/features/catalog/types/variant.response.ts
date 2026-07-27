export interface VariantDetailResponse {
  id: string
  productId: string
  isMaster: boolean
  sku: string
  position: number
  trackInventory: boolean
  weight?: number | null
  weightUnit?: string | null
  height?: number | null
  width?: number | null
  depth?: number | null
  dimensionsUnit?: string | null
  price?: number | null
  costPrice?: number | null
  costCurrency?: string | null
  discontinuedOn?: string | null
  pricesCount: number
  createdAt: string
  updatedAt: string
}

export interface VariantListItemResponse {
  id: string
  productId: string
  isMaster: boolean
  sku: string
  position: number
  trackInventory: boolean
  weight?: number | null
  weightUnit?: string | null
  price?: number | null
  createdAt: string
}
