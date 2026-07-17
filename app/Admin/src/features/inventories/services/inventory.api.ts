import apiClient from '@/shared/api/http/api.client'
import { INVENTORY } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { StockItem, StockItemDetail, StockLocation, StockLocationDetail, InventoryUnit, StockMovement, StockTransfer, StockTransferDetail, StockAdjustmentRequest, CreateStockLocationRequest, CreateStockTransferRequest, InventorySearchParams } from '../types/inventory.types'

export const inventoryApi = {
  stocks: {
    async list(params: InventorySearchParams): Promise<ApiResult<StockItem[]>> {
      return apiClient.get(`${INVENTORY}/stock-items`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockItemDetail>> {
      return apiClient.get(`${INVENTORY}/stock-items/${id}`)
    },
    async create(data: { variantId: string; stockLocationId: string; countOnHand?: number }): Promise<ApiResult<StockItemDetail>> {
      return apiClient.post(`${INVENTORY}/stock-items`, data)
    },
    async update(id: string, data: { countOnHand?: number; backorderable?: boolean; backorderLimit?: number }): Promise<ApiResult<void>> {
      return apiClient.put(`${INVENTORY}/stock-items/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${INVENTORY}/stock-items/${id}`)
    },
    async restock(id: string, data: StockAdjustmentRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-items/${id}/restock`, data)
    },
    async getLowStock(params?: ServerQueryingParameters): Promise<ApiResult<StockItem[]>> {
      return apiClient.get(`${INVENTORY}/stock-items/low-stock`, { params })
    },
    async getSummary(): Promise<ApiResult<any>> {
      return apiClient.get(`${INVENTORY}/stock-items/summary`)
    },
    async bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-items/bulk-adjust`, data)
    },
  },

  locations: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<StockLocation[]>> {
      return apiClient.get(`${INVENTORY}/stock-locations`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.get(`${INVENTORY}/stock-locations/${id}`)
    },
    async create(data: CreateStockLocationRequest): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.post(`${INVENTORY}/stock-locations`, data)
    },
    async update(id: string, data: Partial<CreateStockLocationRequest>): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.put(`${INVENTORY}/stock-locations/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${INVENTORY}/stock-locations/${id}`)
    },
    async setDefault(id: string): Promise<ApiResult<void>> {
      return apiClient.put(`${INVENTORY}/stock-locations/${id}/default`)
    },
  },

  reservations: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<InventoryUnit[]>> {
      return apiClient.get(`${INVENTORY}/stock-reservations`, { params })
    },
    async getById(id: string): Promise<ApiResult<InventoryUnit>> {
      return apiClient.get(`${INVENTORY}/stock-reservations/${id}`)
    },
    async cancel(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-reservations/${id}/cancel`)
    },
  },

  transfers: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<StockTransfer[]>> {
      return apiClient.get(`${INVENTORY}/stock-transfers`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockTransferDetail>> {
      return apiClient.get(`${INVENTORY}/stock-transfers/${id}`)
    },
    async create(data: CreateStockTransferRequest): Promise<ApiResult<StockTransferDetail>> {
      return apiClient.post(`${INVENTORY}/stock-transfers`, data)
    },
    async transfer(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-transfers/${id}/transfer`)
    },
    async receive(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-transfers/${id}/receive`)
    },
    async cancel(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-transfers/${id}/cancel`)
    },
  },

  movements: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<StockMovement[]>> {
      return apiClient.get(`${INVENTORY}/stock-movements`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockMovement>> {
      return apiClient.get(`${INVENTORY}/stock-movements/${id}`)
    },
  },
}
