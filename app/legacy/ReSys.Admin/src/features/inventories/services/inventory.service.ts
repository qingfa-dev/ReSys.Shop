import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type {
  StockLocation,
  StockLocationDetail,
  StockItem,
  StockItemDetail,
  InventoryUnit,
  InventoryUnitSearchParams,
  StockMovement,
  StockMovementSearchParams,
  StockTransfer,
  StockTransferDetail,
  InventorySearchParams,
  StockAdjustmentRequest,
  StockAuditRequest,
  CreateStockLocationRequest,
  CreateStockTransferRequest
} from '../types/inventory.types';

export const inventoryService = {
  // --- Stock Items ---
  async listStocks(params: InventorySearchParams): Promise<ApiResult<StockItem[]>> {
    return apiClient.get('/inventories/stocks', { params });
  },

  async getStockDetail(id: string): Promise<ApiResult<StockItemDetail>> {
    return apiClient.get(`/inventories/stocks/${id}`);
  },

  async adjustStock(id: string, data: StockAdjustmentRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/stocks/${id}/adjust`, data);
  },

  async auditStock(id: string, data: StockAuditRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/stocks/${id}/audit`, data);
  },

  async updateBackorderPolicy(id: string, backorderable: boolean, limit: number): Promise<ApiResult<void>> {
    return apiClient.put(`/inventories/stocks/${id}/backorder-policy`, {
      backorderable,
      backorder_limit: limit
    });
  },

  async deleteStock(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/inventories/stocks/${id}`);
  },

  // --- Inventory Units ---
  async listInventoryUnits(params: InventoryUnitSearchParams): Promise<ApiResult<InventoryUnit[]>> {
    return apiClient.get('/inventories/units', { params });
  },

  async getInventoryUnitDetail(id: string): Promise<ApiResult<InventoryUnit>> {
    return apiClient.get(`/inventories/units/${id}`);
  },

  async updateInventoryUnitSerialNumber(id: string, serialNumber: string): Promise<ApiResult<void>> {
    return apiClient.patch(`/inventories/units/${id}/serial-number`, { serial_number: serialNumber });
  },

  async markInventoryUnitDamaged(id: string): Promise<ApiResult<void>> {
    return apiClient.patch(`/inventories/units/${id}/damaged`);
  },

  async restoreInventoryUnit(id: string): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/units/${id}/restore`);
  },

  // --- Movements (Audit Trail) ---
  async listMovements(params: StockMovementSearchParams): Promise<ApiResult<StockMovement[]>> {
    return apiClient.get('/inventories/movements', { params });
  },

  // --- Locations ---
  async listLocations(params: InventorySearchParams): Promise<ApiResult<StockLocation[]>> {
    return apiClient.get('/inventories/locations', { params });
  },

  async getLocationTree(): Promise<ApiResult<any[]>> {
    return apiClient.get('/inventories/locations/tree');
  },

  async getLocationDetail(id: string): Promise<ApiResult<StockLocationDetail>> {
    return apiClient.get(`/inventories/locations/${id}`);
  },

  async createLocation(data: CreateStockLocationRequest): Promise<ApiResult<StockLocationDetail>> {
    return apiClient.post('/inventories/locations', data);
  },

  async updateLocation(id: string, data: Partial<CreateStockLocationRequest>): Promise<ApiResult<StockLocationDetail>> {
    return apiClient.put(`/inventories/locations/${id}`, data);
  },

  async deleteLocation(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/inventories/locations/${id}`);
  },

  async toggleLocationStatus(id: string, activate: boolean): Promise<ApiResult<void>> {
    return apiClient.patch(`/inventories/locations/${id}/toggle-status`, null, {
      params: { activate }
    });
  },

  // --- Transfers ---
  async listTransfers(params: InventorySearchParams): Promise<ApiResult<StockTransfer[]>> {
    return apiClient.get('/inventories/transfers', { params });
  },

  async getTransferDetail(id: string): Promise<ApiResult<StockTransferDetail>> {
    return apiClient.get(`/inventories/transfers/${id}`);
  },

  async createTransfer(data: CreateStockTransferRequest): Promise<ApiResult<StockTransferDetail>> {
    return apiClient.post('/inventories/transfers', data);
  },

  async addTransferItem(id: string, variantId: string, quantity: number): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/transfers/${id}/items`, {
      variant_id: variantId,
      quantity
    });
  },

  async shipTransfer(id: string): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/transfers/${id}/ship`);
  },

  async receiveTransfer(id: string): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/transfers/${id}/receive`);
  },

  async cancelTransfer(id: string): Promise<ApiResult<void>> {
    return apiClient.post(`/inventories/transfers/${id}/cancel`);
  }
};