import type { InventoryItem, StockStatus, InventoryItemSchemaType, StockStatusSchemaType } from '../types'

export function toInventoryItem(schema: InventoryItemSchemaType): InventoryItem {
  return {
    id: schema.id,
    productId: schema.productId,
    quantity: schema.quantity,
    reserved: schema.reserved,
    available: schema.available,
    warehouse: schema.warehouse,
    lowStockThreshold: schema.lowStockThreshold,
  }
}

export function fromInventoryItem(item: InventoryItem): InventoryItemSchemaType {
  return InventoryItemSchema.parse(item)
}

export function toStockStatus(schema: StockStatusSchemaType): StockStatus {
  return {
    inStock: schema.inStock,
    lowStock: schema.lowStock,
    outOfStock: schema.outOfStock,
    quantity: schema.quantity,
  }
}

export function fromStockStatus(status: StockStatus): StockStatusSchemaType {
  return StockStatusSchema.parse(status)
}

export function isLowStock(item: InventoryItem): boolean {
  return item.available <= item.lowStockThreshold
}

export function isOutOfStock(item: InventoryItem): boolean {
  return item.quantity === 0
}

export function calculateAvailableQuantity(item: InventoryItem): number {
  return Math.max(0, item.quantity - item.reserved)
}

import { InventoryItemSchema, StockStatusSchema } from '../types/schemas'