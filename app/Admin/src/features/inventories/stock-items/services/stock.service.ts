import { stockRepository } from '../api/stock.api'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockItem, StockItemDetail, StockSummary } from '../types/stock-item.response.type'
import type { StockItemQuery } from '../types/stock-item.query.type'
import type { StockAdjustmentRequest } from '../types/stock-item.request.type'

export const stockService = {
  async listStocks(params: StockItemQuery): Promise<ServerPagedResult<StockItem>> {
    return stockRepository.list(params)
  },

  async getStockDetail(id: string): Promise<ServerResult<StockItemDetail>> {
    return stockRepository.getById(id)
  },

  async createStock(data: { variantId: string; stockLocationId: string; countOnHand?: number }): Promise<ServerResult<StockItemDetail>> {
    return stockRepository.create(data)
  },

  async restock(id: string, data: StockAdjustmentRequest): Promise<ServerResult<void>> {
    return stockRepository.restock(id, data) as Promise<ServerResult<void>>
  },

  async deleteStock(id: string): Promise<ServerResult<void>> {
    return stockRepository.delete(id) as Promise<ServerResult<void>>
  },

  async getLowStock(params?: ServerQueryingParameters): Promise<ServerPagedResult<StockItem>> {
    const result = await stockRepository.getLowStock(params)
    if (result.isSuccess) {
      return { isSuccess: true, statusCode: result.statusCode, errors: result.errors, message: result.message, metadata: result.metadata, items: result.value, page: 1, pageSize: result.value.length, totalCount: result.value.length }
    }
    return { isSuccess: false, statusCode: result.statusCode, errors: result.errors, message: result.message, metadata: result.metadata, items: [], page: 0, pageSize: 0, totalCount: 0 } as ServerPagedResult<StockItem>
  },

  async getStockSummary(): Promise<ServerResult<StockSummary[]>> {
    return stockRepository.getSummary()
  },

  async bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ServerResult<void>> {
    return stockRepository.bulkAdjust(data)
  },
}
