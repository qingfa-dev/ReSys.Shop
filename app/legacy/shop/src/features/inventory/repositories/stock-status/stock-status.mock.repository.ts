import type { StockStatusResponse, InventoryItemResponse } from '../../types/response'
import type { IStockStatusRepository } from './stock-status.repository.interface'
import type { Result } from '@/core/models/result'

const mockInventory: Record<string, InventoryItemResponse> = {
  'prod-1': { id: 'inv-1', productId: 'prod-1', quantity: 100, reserved: 10, available: 90, warehouse: 'WH-NYC', lowStockThreshold: 20 },
  'prod-2': { id: 'inv-2', productId: 'prod-2', quantity: 10, reserved: 5, available: 5, warehouse: 'WH-LAX', lowStockThreshold: 15 },
  'prod-3': { id: 'inv-3', productId: 'prod-3', quantity: 0, reserved: 0, available: 0, warehouse: 'WH-CHI', lowStockThreshold: 10 },
}

export class MockStockStatusRepository implements IStockStatusRepository {
  async getByProductId(productId: string): Promise<Result<StockStatusResponse>> {
    const item = mockInventory[productId]
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
}

export const mockStockStatusRepository = new MockStockStatusRepository()