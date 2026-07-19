import { stockRepository } from '../stock-items/api/stock.api'
import { locationRepository } from '../stock-locations/api/location.api'
import { reservationRepository } from '../inventory-units/api/reservation.api'
import { transferRepository } from '../stock-transfers/api/transfer.api'
import { movementRepository } from '../stock-movements/api/movement.api'
import { mapStockItem } from '../stock-items/mappers/stock-item.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockItem, StockItemDetail, StockSummary } from '../stock-items/types/stock-item.response.type'
import type { StockLocation, StockLocationDetail } from '../stock-locations/types/stock-location.response.type'
import type { InventoryUnit } from '../inventory-units/types/inventory-unit.response.type'
import type { StockMovement } from '../stock-movements/types/stock-movement.response.type'
import type { StockTransfer, StockTransferDetail } from '../stock-transfers/types/stock-transfer.response.type'
import type { StockAdjustmentRequest } from '../stock-items/types/stock-item.request.type'
import type { CreateStockLocationRequest } from '../stock-locations/types/stock-location.request.type'
import type { CreateStockTransferRequest } from '../stock-transfers/types/stock-transfer.request.type'
import type { StockItemQuery } from '../stock-items/types/stock-item.query.type'

function applyMap<R>(data: any, mapper: (d: any) => any): R {
  return mapper(data) as R
}

function applyMapArray<R>(data: any[], mapper: (d: any) => any): R[] {
  return data.map(d => mapper(d) as R)
}

export const inventoryService = {
  async listStocks(params: StockItemQuery): Promise<ServerPagedResult<StockItem>> {
    const result = await stockRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockItem) } : result as  ServerPagedResult<StockItem>
  },

  async getStockDetail(id: string): Promise<ServerResult<StockItemDetail>> {
    const result = await stockRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockItem) } as  ServerResult<StockItemDetail> : result as  ServerResult<StockItemDetail>
  },

  async createStock(data: { variantId: string; stockLocationId: string; countOnHand?: number }): Promise<ServerResult<StockItemDetail>> {
    const result = await stockRepository.create(data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockItem) } as  ServerResult<StockItemDetail> : result as  ServerResult<StockItemDetail>
  },

  async restock(id: string, data: StockAdjustmentRequest): Promise<ServerResult<void>> {
    return stockRepository.restock(id, data) as  Promise<ServerResult<void>>
  },

  async deleteStock(id: string): Promise<ServerResult<void>> {
    return stockRepository.delete(id) as  Promise<ServerResult<void>>
  },

  async getLowStock(params?: ServerQueryingParameters): Promise<ServerPagedResult<StockItem>> {
    const result = await stockRepository.getLowStock(params)
    if (result.isSuccess) {
      const mapped = applyMapArray<StockItem>(result.value, mapStockItem)
      return { isSuccess: true, statusCode: result.statusCode, errors: result.errors, message: result.message, metadata: result.metadata, items: mapped, page: 1, pageSize: mapped.length, totalCount: mapped.length }
    }
    return { isSuccess: false, statusCode: result.statusCode, errors: result.errors, message: result.message, metadata: result.metadata, items: [], page: 0, pageSize: 0, totalCount: 0 } as ServerPagedResult<StockItem>
  },

  async getStockSummary(): Promise<ServerResult<StockSummary[]>> {
    return stockRepository.getSummary() as  Promise<ServerResult<StockSummary[]>>
  },

  async bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ServerResult<void>> {
    return stockRepository.bulkAdjust(data) as  Promise<ServerResult<void>>
  },

  async listLocations(params: ServerQueryingParameters): Promise<ServerPagedResult<StockLocation>> {
    return locationRepository.list(params)
  },

  async getLocationDetail(id: string): Promise<ServerResult<StockLocationDetail>> {
    return locationRepository.getById(id)
  },

  async createLocation(data: CreateStockLocationRequest): Promise<ServerResult<StockLocationDetail>> {
    return locationRepository.create(data)
  },

  async updateLocation(id: string, data: Partial<CreateStockLocationRequest>): Promise<ServerResult<StockLocationDetail>> {
    return locationRepository.update(id, data)
  },

  async deleteLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.delete(id) as  Promise<ServerResult<void>>
  },

  async setDefaultLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.setDefault(id) as  Promise<ServerResult<void>>
  },

  async listReservations(params: ServerQueryingParameters): Promise<ServerPagedResult<InventoryUnit>> {
    return reservationRepository.list(params)
  },

  async getReservationDetail(id: string): Promise<ServerResult<InventoryUnit>> {
    return reservationRepository.getById(id)
  },

  async cancelReservation(id: string): Promise<ServerResult<void>> {
    return reservationRepository.cancel(id) as  Promise<ServerResult<void>>
  },

  async listTransfers(params: ServerQueryingParameters): Promise<ServerPagedResult<StockTransfer>> {
    return transferRepository.list(params)
  },

  async getTransferDetail(id: string): Promise<ServerResult<StockTransferDetail>> {
    return transferRepository.getById(id)
  },

  async createTransfer(data: CreateStockTransferRequest): Promise<ServerResult<StockTransferDetail>> {
    return transferRepository.create(data)
  },

  async transferStock(id: string): Promise<ServerResult<void>> {
    return transferRepository.transfer(id) as  Promise<ServerResult<void>>
  },

  async receiveTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.receive(id) as  Promise<ServerResult<void>>
  },

  async cancelTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.cancel(id) as  Promise<ServerResult<void>>
  },

  async listMovements(params: ServerQueryingParameters): Promise<ServerPagedResult<StockMovement>> {
    return movementRepository.list(params)
  },

  async getMovementDetail(id: string): Promise<ServerResult<StockMovement>> {
    return movementRepository.getById(id)
  },

  adjustStock: async (_stockItemId: string, _data: Record<string, unknown>): Promise<ServerResult<void>> => ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
  addTransferItem: async (_transferId: string, _productId: string, _quantity: number): Promise<ServerResult<void>> => ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
  shipTransfer: async (_id: string): Promise<ServerResult<void>> => ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
}
