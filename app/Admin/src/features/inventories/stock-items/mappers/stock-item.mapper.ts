import type { StockItem, StockItemDetail } from '../types/stock-item.response.type'

export function mapStockItem(data: StockItem): StockItem & { countAvailable: number } {
  return {
    id: data.id,
    stockLocationId: data.stockLocationId,
    variantId: data.variantId,
    countOnHand: data.countOnHand,
    backorderable: data.backorderable,
    sku: data.sku,
    variantName: data.variantName,
    stockLocationName: data.stockLocationName,
    countAvailable: data.countOnHand,
  }
}

export function mapStockItemDetail(data: StockItemDetail): StockItemDetail & { countAvailable: number } {
  return {
    ...mapStockItem(data),
    createdAtUtc: data.createdAtUtc,
    modifiedAtUtc: data.modifiedAtUtc,
    createdBy: data.createdBy,
    modifiedBy: data.modifiedBy,
  }
}
