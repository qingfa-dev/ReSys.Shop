import type { Result } from '@/core/models/result'
import type { InventoryItem } from '../../types'
import type { Reservation } from '../../repositories/inventory-item/inventory-item.repository.interface'

export interface IInventoryItemService {
  getInventory(productId: string): Promise<Result<InventoryItem>>
  reserveStock(variantId: string, quantity: number): Promise<Result<InventoryItem>>
  getReservations(): Promise<Result<Reservation[]>>
}