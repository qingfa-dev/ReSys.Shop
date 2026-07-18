import { stockRepository } from '../api/stock.api'
import { mapStockItem } from '../mappers/stock-item.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockItem, StockItemDetail, StockSummary } from '../types/StockItem.Response.Type'
import type { StockItemQuery } from '../types/StockItem.Query.Type'
import type { StockAdjustmentRequest } from '../types/StockItem.Request.Type'

function applyMap<T, R>(data: T, mapper: (d: T) => R): R {
  return mapper(data) as R
}

function applyMapArray<T, R>(data: T[], mapper: (d: T) => R): R[] {
  return data.map(d => mapper(d) as R)
}

export const stockService = {
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

  async getStockSummary(): Promise<ServerResult<StockSummary[]>> {
    return stockRepository.getSummary() as unknown as Promise<ServerResult<StockSummary[]>>
  },

  async bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ServerResult<void>> {
    return stockRepository.bulkAdjust(data) as unknown as Promise<ServerResult<void>>
  },
}
