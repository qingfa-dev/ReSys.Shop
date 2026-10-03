export interface StockMovementResponse {
  id: string
  stockItemId: string
  variantSku?: string | null
  locationId: string
  locationName?: string | null
  quantity: number
  direction: string
  reason: string
  reference?: string | null
  createdAt: string
}
