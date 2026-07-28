import type { InventoryItemResponse, StockStatusResponse } from '../../types/response'
import type { IInventoryItemRepository, Reservation } from './inventory-item.repository.interface'
import type { Result } from '@/core/models/result'

const initialInventoryItems: InventoryItemResponse[] = [
  { id: 'inv-1', productId: 'prod-1', quantity: 100, reserved: 10, available: 90, warehouse: 'WH-NYC', lowStockThreshold: 20 },
  { id: 'inv-2', productId: 'prod-2', quantity: 50, reserved: 5, available: 45, warehouse: 'WH-LAX', lowStockThreshold: 15 },
  { id: 'inv-3', productId: 'prod-3', quantity: 5, reserved: 0, available: 5, warehouse: 'WH-CHI', lowStockThreshold: 10 },
]

const mockInventoryItems: InventoryItemResponse[] = JSON.parse(JSON.stringify(initialInventoryItems))

export class MockInventoryItemRepository implements IInventoryItemRepository {
  static reset() {
    mockInventoryItems.length = 0
    initialInventoryItems.forEach(i => mockInventoryItems.push({ ...i }))
  }

  async getById<T = any>(id: string): Promise<Result<T>> {
    const item = mockInventoryItems.find(i => i.id === id)
    if (!item) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Inventory item not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: item as T }
  }

  async getStockStatus(productId: string): Promise<Result<StockStatusResponse>> {
    const item = mockInventoryItems.find(i => i.productId === productId)
    if (!item) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Product not found' }
    }
    const status: StockStatusResponse = {
      inStock: item.quantity > item.lowStockThreshold,
      lowStock: item.quantity > 0 && item.quantity <= item.lowStockThreshold,
      outOfStock: item.quantity === 0,
      quantity: item.quantity,
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: status }
  }

  async reserveStock(variantId: string, quantity: number, _cartToken: string): Promise<Result<any>> {
    const item = mockInventoryItems.find(i => i.productId === variantId)
    if (!item || item.available < quantity) {
      return { isSuccess: false, isFailure: true, statusCode: 400, message: 'Insufficient stock' }
    }
    item.reserved += quantity
    item.available -= quantity
    return { isSuccess: true, isFailure: false, statusCode: 200, data: item }
  }

  async getReservations(cartToken: string): Promise<Result<Reservation[]>> {
    const reservations: Reservation[] = [
      { id: 'res-1', variantId: 'prod-1', quantity: 2, expiresAt: '2026-07-29T00:00:00Z' },
      { id: 'res-2', variantId: 'prod-2', quantity: 1, expiresAt: '2026-07-29T00:00:00Z' },
    ]
    return { isSuccess: true, isFailure: false, statusCode: 200, data: reservations }
  }
}

export const mockInventoryItemRepository = new MockInventoryItemRepository()