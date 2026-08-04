import type { Result } from '@/core/models/result'

export interface Reservation {
  id: string
  variantId: string
  quantity: number
  expiresAt: string
}

export interface IInventoryItemRepository {
  getById<T = any>(id: string): Promise<Result<T>>
  getStockStatus(productId: string): Promise<Result<any>>
  reserveStock(variantId: string, quantity: number, cartToken: string): Promise<Result<any>>
  getReservations(cartToken: string): Promise<Result<Reservation[]>>
}