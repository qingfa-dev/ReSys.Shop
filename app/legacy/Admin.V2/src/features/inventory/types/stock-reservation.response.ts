export interface StockReservationResponse {
  id: string
  orderId?: string | null
  orderNumber?: string | null
  variantId: string
  variantSku?: string | null
  quantity: number
  status: string
  expiresAt?: string | null
  createdAt: string
  updatedAt: string
}
