import type { StockItem, StockItemDetail, StockLocation, StockLocationDetail, StockTransfer, StockTransferDetail, InventoryUnit, StockMovement } from '../types/inventory.domain.types'

export function mapStockItem<T extends StockItem>(data: T): T {
  return data
}

export function mapStockLocation<T extends StockLocation>(data: T): T {
  return data
}

export function mapStockTransfer<T extends StockTransfer>(data: T): T {
  return data
}

export function mapInventoryUnit(data: InventoryUnit): InventoryUnit {
  return data
}

export function mapStockMovement(data: StockMovement): StockMovement {
  return data
}
