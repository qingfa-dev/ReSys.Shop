import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { IInventoryItemRepository, Reservation } from './inventory-item.repository.interface'

export class InventoryItemApiRepository extends BaseRepository implements IInventoryItemRepository {
  async getById<T = any>(id: string): Promise<Result<T>> {
    return this.get<T>(`/api/storefront/availability/${id}`)
  }

  async getStockStatus(productId: string): Promise<Result<any>> {
    return this.get<any>(`/api/storefront/availability/${productId}`)
  }

  async reserveStock(variantId: string, quantity: number, cartToken: string): Promise<Result<any>> {
    return this.post<any>('/api/storefront/cart/reserve', { variantId, quantity, cartToken })
  }

  async getReservations(cartToken: string): Promise<Result<Reservation[]>> {
    return this.get<Reservation[]>('/api/storefront/cart/reserve', { filter: `cartToken:${cartToken}` })
  }
}

export const inventoryItemApiRepository = new InventoryItemApiRepository()