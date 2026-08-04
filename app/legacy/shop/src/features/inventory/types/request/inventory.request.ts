export interface GetStockStatusRequest {
  productId: string
}

export interface UpdateInventoryRequest {
  quantity: number
  reason?: string
}

export interface ReserveStockRequest {
  productId: string
  quantity: number
  orderId?: string
}

export interface ReleaseStockRequest {
  productId: string
  quantity: number
  orderId?: string
}

export interface GetLowStockRequest {
  threshold?: number
  warehouse?: string
}