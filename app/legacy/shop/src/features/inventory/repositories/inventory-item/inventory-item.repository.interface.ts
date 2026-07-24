import type { Result } from '@/core/models/result'
import type { InventoryItemResponse } from '../../types/response'

export interface IInventoryItemRepository {
  getAll(threshold?: number): Promise<Result<InventoryItemResponse[]>>
  getById<T = InventoryItemResponse>(id: string): Promise<Result<T>>
  getStockStatus(productId: string): Promise<Result<any>>
  updateQuantity(productId: string, quantity: number): Promise<Result<InventoryItemResponse>>
  reserveStock(productId: string, quantity: number): Promise<Result<InventoryItemResponse>>
  releaseStock(productId: string, quantity: number): Promise<Result<InventoryItemResponse>>
}