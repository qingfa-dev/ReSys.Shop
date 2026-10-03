import type { Result } from '@/core/models/result'
import type { InventoryItem, StockStatus } from '../types'

export interface IInventoryService {
  getInventory(productId: string): Promise<Result<InventoryItem>>
  getStockStatus(productId: string): Promise<Result<StockStatus>>
  getLowStockProducts(threshold?: number): Promise<Result<InventoryItem[]>>
  updateQuantity(productId: string, quantity: number): Promise<Result<InventoryItem>>
  reserveStock(productId: string, quantity: number): Promise<Result<InventoryItem>>
  releaseStock(productId: string, quantity: number): Promise<Result<InventoryItem>>
}