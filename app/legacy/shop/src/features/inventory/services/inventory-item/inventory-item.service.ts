import { inventoryItemApiRepository } from '../../repositories/inventory-item/inventory-item.api'
import { mockInventoryItemRepository } from '../../repositories/inventory-item/inventory-item.mock.repository'
import type { IInventoryItemService } from './inventory-item.service.interface'
import type { InventoryItem } from '../../types'
import type { Result } from '@/core/models/result'
import { toInventoryItem } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class InventoryItemService implements IInventoryItemService {
  private readonly inventoryItemRepository = USE_MOCK ? mockInventoryItemRepository : inventoryItemApiRepository

  async getInventory(productId: string): Promise<Result<InventoryItem>> {
    const response = await this.inventoryItemRepository.getById(productId)
    return resultMap(response, toInventoryItem)
  }

  async getLowStockProducts(threshold?: number): Promise<Result<InventoryItem[]>> {
    const response = await this.inventoryItemRepository.getAll(threshold)
    if (response.isFailure) {
      return response as unknown as Result<InventoryItem[]>
    }
    return resultMap(response, (data) => data.map(toInventoryItem))
  }

  async updateQuantity(productId: string, quantity: number): Promise<Result<InventoryItem>> {
    const response = await this.inventoryItemRepository.updateQuantity(productId, quantity)
    return resultMap(response, toInventoryItem)
  }

  async reserveStock(productId: string, quantity: number): Promise<Result<InventoryItem>> {
    const response = await this.inventoryItemRepository.reserveStock(productId, quantity)
    return resultMap(response, toInventoryItem)
  }

  async releaseStock(productId: string, quantity: number): Promise<Result<InventoryItem>> {
    const response = await this.inventoryItemRepository.releaseStock(productId, quantity)
    return resultMap(response, toInventoryItem)
  }
}

export const inventoryItemService = new InventoryItemService()