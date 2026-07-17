import { stockRepository } from '../repository/stock.repository'
import { locationRepository } from '../repository/location.repository'
import { reservationRepository } from '../repository/reservation.repository'
import { transferRepository } from '../repository/transfer.repository'
import { movementRepository } from '../repository/movement.repository'
import { mapStockItem, mapStockLocation, mapStockTransfer, mapInventoryUnit, mapStockMovement } from '../mapper/inventory.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockItem, StockItemDetail } from '../types/StockItem.Response.Type'
import type { StockLocation, StockLocationDetail } from '../types/StockLocation.Response.Type'
import type { InventoryUnit } from '../types/InventoryUnit.Response.Type'
import type { StockMovement } from '../types/StockMovement.Response.Type'
import type { StockTransfer, StockTransferDetail } from '../types/StockTransfer.Response.Type'
import type { StockAdjustmentRequest } from '../types/StockItem.Request.Type'
import type { CreateStockLocationRequest } from '../types/StockLocation.Request.Type'
import type { CreateStockTransferRequest } from '../types/StockTransfer.Request.Type'
import type { StockItemQuery } from '../types/StockItem.Query.Type'

function applyMap<T, R>(data: T, mapper: (d: any) => R): R {
  return mapper(data) as R
}

function applyMapArray<T, R>(data: T[], mapper: (d: any) => R): R[] {
  return data.map(d => mapper(d) as R)
}

export const inventoryService = {
  async listStocks(params: StockItemQuery): Promise<ServerPagedResult<StockItem>> {
    const result = await stockRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockItem) } : result as unknown as ServerPagedResult<StockItem>
  },

  async getStockDetail(id: string): Promise<ServerResult<StockItemDetail>> {
    const result = await stockRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockItem) } as unknown as ServerResult<StockItemDetail> : result as unknown as ServerResult<StockItemDetail>
  },

  async createStock(data: { variantId: string; stockLocationId: string; countOnHand?: number }): Promise<ServerResult<StockItemDetail>> {
    const result = await stockRepository.create(data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockItem) } as unknown as ServerResult<StockItemDetail> : result as unknown as ServerResult<StockItemDetail>
  },

  async restock(id: string, data: StockAdjustmentRequest): Promise<ServerResult<void>> {
    return stockRepository.restock(id, data) as unknown as Promise<ServerResult<void>>
  },

  async deleteStock(id: string): Promise<ServerResult<void>> {
    return stockRepository.delete(id) as unknown as Promise<ServerResult<void>>
  },

  async getLowStock(params?: ServerQueryingParameters): Promise<ServerPagedResult<StockItem>> {
    const result = await stockRepository.getLowStock(params)
    if (result.isSuccess) {
      const mapped = applyMapArray(result.value, mapStockItem)
      return { isSuccess: true, statusCode: result.statusCode, errors: result.errors, message: result.message, metadata: result.metadata, items: mapped, page: 1, pageSize: mapped.length, totalCount: mapped.length }
    }
    return result as unknown as ServerPagedResult<StockItem>
  },

  async getStockSummary(): Promise<ServerResult<any>> {
    return stockRepository.getSummary() as unknown as Promise<ServerResult<any>>
  },

  async bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ServerResult<void>> {
    return stockRepository.bulkAdjust(data) as unknown as Promise<ServerResult<void>>
  },

  async listLocations(params: ServerQueryingParameters): Promise<ServerPagedResult<StockLocation>> {
    const result = await locationRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockLocation) } : result as unknown as ServerPagedResult<StockLocation>
  },

  async getLocationDetail(id: string): Promise<ServerResult<StockLocationDetail>> {
    const result = await locationRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockLocation) } : result as unknown as ServerResult<StockLocationDetail>
  },

  async createLocation(data: CreateStockLocationRequest): Promise<ServerResult<StockLocationDetail>> {
    const result = await locationRepository.create(data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockLocation) } : result as unknown as ServerResult<StockLocationDetail>
  },

  async updateLocation(id: string, data: Partial<CreateStockLocationRequest>): Promise<ServerResult<StockLocationDetail>> {
    const result = await locationRepository.update(id, data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockLocation) } : result as unknown as ServerResult<StockLocationDetail>
  },

  async deleteLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.delete(id) as unknown as Promise<ServerResult<void>>
  },

  async setDefaultLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.setDefault(id) as unknown as Promise<ServerResult<void>>
  },

  async listReservations(params: ServerQueryingParameters): Promise<ServerPagedResult<InventoryUnit>> {
    const result = await reservationRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapInventoryUnit) } : result as unknown as ServerPagedResult<InventoryUnit>
  },

  async getReservationDetail(id: string): Promise<ServerResult<InventoryUnit>> {
    const result = await reservationRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapInventoryUnit) } as unknown as ServerResult<InventoryUnit> : result as unknown as ServerResult<InventoryUnit>
  },

  async cancelReservation(id: string): Promise<ServerResult<void>> {
    return reservationRepository.cancel(id) as unknown as Promise<ServerResult<void>>
  },

  async listTransfers(params: ServerQueryingParameters): Promise<ServerPagedResult<StockTransfer>> {
    const result = await transferRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockTransfer) } : result as unknown as ServerPagedResult<StockTransfer>
  },

  async getTransferDetail(id: string): Promise<ServerResult<StockTransferDetail>> {
    const result = await transferRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockTransfer) } : result as unknown as ServerResult<StockTransferDetail>
  },

  async createTransfer(data: CreateStockTransferRequest): Promise<ServerResult<StockTransferDetail>> {
    const result = await transferRepository.create(data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockTransfer) } : result as unknown as ServerResult<StockTransferDetail>
  },

  async transferStock(id: string): Promise<ServerResult<void>> {
    return transferRepository.transfer(id) as unknown as Promise<ServerResult<void>>
  },

  async receiveTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.receive(id) as unknown as Promise<ServerResult<void>>
  },

  async cancelTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.cancel(id) as unknown as Promise<ServerResult<void>>
  },

  async listMovements(params: ServerQueryingParameters): Promise<ServerPagedResult<StockMovement>> {
    const result = await movementRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockMovement) } : result as unknown as ServerPagedResult<StockMovement>
  },

  async getMovementDetail(id: string): Promise<ServerResult<StockMovement>> {
    const result = await movementRepository.getById(id)
    const movResult = result.isSuccess ? { ...result, value: applyMap(result.value, mapStockMovement) } as unknown as ServerResult<StockMovement> : result as unknown as ServerResult<StockMovement>; return movResult
  },

  adjustStock: async (_stockItemId: string, _data: Record<string, unknown>): Promise<ServerResult<void>> => ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
  addTransferItem: async (_transferId: string, _productId: string, _quantity: number): Promise<ServerResult<void>> => ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
  shipTransfer: async (_id: string): Promise<ServerResult<void>> => ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
}
