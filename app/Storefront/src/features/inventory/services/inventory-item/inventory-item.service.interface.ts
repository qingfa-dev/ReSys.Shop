import type { Result } from '@/core/models/result'
import type { InventoryItem } from '../../types'

export interface IInventoryItemService {
  getInventory(productId: string): Promise<Result<InventoryItem>>
  getLowStockProducts(threshold?: number): Promise<Result<InventoryItem[]>>
  updateQuantity(productId: string, quantity: number): Promise<Result<InventoryItem>>
  reserveStock(productId: string, quantity: number): Promise<Result<InventoryItem>>
  releaseStock(productId: string, quantity: number): Promise<Result<InventoryItem>>
}