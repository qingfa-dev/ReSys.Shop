import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { InventoryItemResponse, StockStatusResponse } from '../../types/response'
import type { IInventoryItemRepository } from './inventory-item.repository.interface'

export class InventoryItemApiRepository extends BaseRepository implements IInventoryItemRepository {
  async getAll(threshold = 10): Promise<Result<InventoryItemResponse[]>> {
    return this.get<InventoryItemResponse[]>('/inventory/low-stock', { filter: `threshold:${threshold}` })
  }

  async getById<T = InventoryItemResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/inventory/${id}`)
  }

  async getStockStatus(productId: string): Promise<Result<StockStatusResponse>> {
    return this.get<StockStatusResponse>(`/inventory/${productId}/stock-status`)
  }

  async updateQuantity(productId: string, quantity: number): Promise<Result<InventoryItemResponse>> {
    return this.patchPartial<InventoryItemResponse>('/inventory', productId, { quantity })
  }

  async reserveStock(productId: string, quantity: number): Promise<Result<InventoryItemResponse>> {
    return this.post<InventoryItemResponse>(`/inventory/${productId}/reserve`, { quantity })
  }

  async releaseStock(productId: string, quantity: number): Promise<Result<InventoryItemResponse>> {
    return this.post<InventoryItemResponse>(`/inventory/${productId}/release`, { quantity })
  }
}

export const inventoryItemApiRepository = new InventoryItemApiRepository()