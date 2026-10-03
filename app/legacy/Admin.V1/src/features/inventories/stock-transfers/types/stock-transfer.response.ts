export interface StockTransfer {
  id: string; sourceLocationId: string; destinationLocationId: string
  sourceLocationName: string; destinationLocationName: string
  status: number; reference: string; notes: string | null
  createdAtUtc: string; createdBy: string | null
}
export interface StockTransferItem { variantId: string; sku: string; variantName: string; quantity: number }
export interface StockTransferDetail extends StockTransfer { items: StockTransferItem[] }
