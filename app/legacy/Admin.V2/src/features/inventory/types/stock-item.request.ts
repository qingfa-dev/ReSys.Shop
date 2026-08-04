import type { CreateStockItemForm, UpdateStockItemForm } from '../schemas'

export type CreateStockItemRequest = CreateStockItemForm
export type UpdateStockItemRequest = UpdateStockItemForm

export interface BulkAdjustItem {
  stockItemId: string
  quantity: number
}

export interface BulkAdjustRequest {
  items: BulkAdjustItem[]
}

export interface RestockRequest {
  quantity: number
}
