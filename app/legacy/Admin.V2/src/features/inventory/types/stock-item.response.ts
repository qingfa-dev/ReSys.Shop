export interface StockItemResponse {
  id: string
  variantId?: string | null
  variantSku?: string | null
  variantName?: string | null
  locationId: string
  locationName?: string | null
  quantity: number
  reservedQuantity: number
  availableQuantity: number
  lowStockThreshold?: number | null
  isLowStock: boolean
  lastRestockedAt?: string | null
  createdAt: string
  updatedAt: string
}

export interface StockSummaryResponse {
  totalItems: number
  totalQuantity: number
  lowStockCount: number
  outOfStockCount: number
  totalLocations: number
}
