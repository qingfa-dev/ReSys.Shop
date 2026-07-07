import apiClient from '@/shared/api/http/api.client'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { StockItem, StockItemDetail, StockLocation, StockLocationDetail, InventoryUnit, InventoryUnitSearchParams, StockMovement, StockMovementSearchParams, StockTransfer, StockTransferDetail, InventorySearchParams, StockAdjustmentRequest, StockAuditRequest, CreateStockLocationRequest, CreateStockTransferRequest } from '../types/inventory.types'

export const inventoryApi = {
  stocks: {
    async list(params: InventorySearchParams): Promise<ApiResult<StockItem[]>> {
      return apiClient.get('/inventories/stocks', { params })
    },
    async getById(id: string): Promise<ApiResult<StockItemDetail>> {
      return apiClient.get(`/inventories/stocks/${id}`)
    },
    async adjust(id: string, data: StockAdjustmentRequest): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/stocks/${id}/adjust`, data)
    },
    async audit(id: string, data: StockAuditRequest): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/stocks/${id}/audit`, data)
    },
    async updateBackorderPolicy(id: string, backorderable: boolean, limit: number): Promise<ApiResult<void>> {
      return apiClient.put(`/inventories/stocks/${id}/backorder-policy`, { backorderable, backorder_limit: limit })
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`/inventories/stocks/${id}`)
    },
  },

  units: {
    async list(params: InventoryUnitSearchParams): Promise<ApiResult<InventoryUnit[]>> {
      return apiClient.get('/inventories/units', { params })
    },
    async getById(id: string): Promise<ApiResult<InventoryUnit>> {
      return apiClient.get(`/inventories/units/${id}`)
    },
    async updateSerialNumber(id: string, serialNumber: string): Promise<ApiResult<void>> {
      return apiClient.patch(`/inventories/units/${id}/serial-number`, { serial_number: serialNumber })
    },
    async markDamaged(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`/inventories/units/${id}/damaged`)
    },
    async restore(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/units/${id}/restore`)
    },
  },

  movements: {
    async list(params: StockMovementSearchParams): Promise<ApiResult<StockMovement[]>> {
      return apiClient.get('/inventories/movements', { params })
    },
  },

  locations: {
    async list(params: InventorySearchParams): Promise<ApiResult<StockLocation[]>> {
      return apiClient.get('/inventories/locations', { params })
    },
    async getTree(): Promise<ApiResult<any[]>> {
      return apiClient.get('/inventories/locations/tree')
    },
    async getById(id: string): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.get(`/inventories/locations/${id}`)
    },
    async create(data: CreateStockLocationRequest): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.post('/inventories/locations', data)
    },
    async update(id: string, data: Partial<CreateStockLocationRequest>): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.put(`/inventories/locations/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`/inventories/locations/${id}`)
    },
    async toggleStatus(id: string, activate: boolean): Promise<ApiResult<void>> {
      return apiClient.patch(`/inventories/locations/${id}/toggle-status`, null, { params: { activate } })
    },
  },

  transfers: {
    async list(params: InventorySearchParams): Promise<ApiResult<StockTransfer[]>> {
      return apiClient.get('/inventories/transfers', { params })
    },
    async getById(id: string): Promise<ApiResult<StockTransferDetail>> {
      return apiClient.get(`/inventories/transfers/${id}`)
    },
    async create(data: CreateStockTransferRequest): Promise<ApiResult<StockTransferDetail>> {
      return apiClient.post('/inventories/transfers', data)
    },
    async addItem(id: string, variantId: string, quantity: number): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/transfers/${id}/items`, { variant_id: variantId, quantity })
    },
    async ship(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/transfers/${id}/ship`)
    },
    async receive(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/transfers/${id}/receive`)
    },
    async cancel(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`/inventories/transfers/${id}/cancel`)
    },
  },
}
