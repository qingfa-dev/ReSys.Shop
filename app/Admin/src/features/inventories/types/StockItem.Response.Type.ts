export interface StockItem {
  id: string; variantId: string; sku: string; variantName: string
  stockLocationId: string; stockLocationName: string; countOnHand: number
  quantityReserved?: number; countAvailable?: number; backorderable: boolean
}
export interface StockItemDetail extends StockItem {
  backorderLimit: number; createdAtUtc: string; modifiedAtUtc: string | null
}
