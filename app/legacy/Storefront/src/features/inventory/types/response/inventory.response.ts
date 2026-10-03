import type { Result } from '@/core/models/result'
import type { InventoryItemSchemaType, StockStatusSchemaType } from '../schemas'

export interface InventoryItemResponse extends InventoryItemSchemaType {}
export interface StockStatusResponse extends StockStatusSchemaType {}

export interface GetStockStatusResponse {
  productId: string
  status: StockStatusResponse
}

export interface UpdateInventoryResponse {
  item: InventoryItemResponse
  previousQuantity: number
  updatedAt: string
}

export interface ReserveStockResponse {
  item: InventoryItemResponse
  reservedAt: string
  expiresAt: string
}

export interface ReleaseStockResponse {
  item: InventoryItemResponse
  releasedAt: string
}

export interface GetLowStockResponse {
  items: InventoryItemResponse[]
  threshold: number
  totalCount: number
}

export type InventorySingleResponse = Result<InventoryItemResponse>
export type InventoryListResponse = Result<InventoryItemResponse[]>