import type { StockItem, StockItemDetail } from '../types/StockItem.Response.Type'
import type { StockLocation, StockLocationDetail } from '../types/StockLocation.Response.Type'
import type { StockTransfer, StockTransferDetail } from '../types/StockTransfer.Response.Type'
import type { InventoryUnit } from '../types/InventoryUnit.Response.Type'
import type { StockMovement } from '../types/StockMovement.Response.Type'

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
