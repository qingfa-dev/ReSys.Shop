export interface StockItem {
  id: string; stockLocationId: string; variantId: string
  countOnHand: number; backorderable: boolean
  sku?: string | null; variantName?: string | null; stockLocationName?: string | null
}
export interface StockItemDetail extends StockItem {
  createdAtUtc: string; modifiedAtUtc: string | null
  createdBy?: string | null; modifiedBy?: string | null
}

export interface LocationBreakdown {
  locationId: string; locationName: string; countOnHand: number; reserved: number; available: number; isLowStock: boolean
}

export interface StockSummary {
  variantId: string; totalOnHand: number; totalReserved: number; totalAvailable: number; locationBreakdown: LocationBreakdown[]
}
