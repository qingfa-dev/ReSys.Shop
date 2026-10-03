import type { StockAdjustmentParameters } from '../types/stock-item.field'
export type StockAdjustmentRequest = StockAdjustmentParameters
export interface StockAuditRequest { physicalCount: number; reason?: string; reference?: string }

export interface CreateStockItemRequest {
  variantId: string
  stockLocationId: string
  countOnHand?: number
}

export interface UpdateStockItemRequest {
  countOnHand?: number
  backorderable?: boolean
  backorderLimit?: number
}

export interface BulkAdjustItem {
  id: string
  quantity: number
  type: number
}

export interface BulkAdjustRequest {
  items: BulkAdjustItem[]
}
