import { inventoryItemApiRepository } from '../../repositories/inventory-item/inventory-item.api'
import { mockInventoryItemRepository } from '../../repositories/inventory-item/inventory-item.mock.repository'
import type { IInventoryItemService } from './inventory-item.service.interface'
import type { InventoryItem } from '../../types'
import type { Reservation } from '../../repositories/inventory-item/inventory-item.repository.interface'
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

  async reserveStock(variantId: string, quantity: number): Promise<Result<InventoryItem>> {
    const cartToken = localStorage.getItem('cartToken') || ''
    const response = await this.inventoryItemRepository.reserveStock(variantId, quantity, cartToken)
    return resultMap(response, toInventoryItem)
  }

  async getReservations(): Promise<Result<Reservation[]>> {
    const cartToken = localStorage.getItem('cartToken') || ''
    return this.inventoryItemRepository.getReservations(cartToken)
  }
}

export const inventoryItemService = new InventoryItemService()