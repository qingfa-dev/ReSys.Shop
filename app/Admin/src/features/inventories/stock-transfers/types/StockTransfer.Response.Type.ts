export type TransferState = 'Draft' | 'InTransit' | 'Received' | 'Canceled'
export interface StockTransfer {
  id: string; number: string; referenceNumber: string; sourceLocationId: string
  sourceLocationName: string; destinationLocationId: string; destinationLocationName: string
  state: TransferState; createdAtUtc: string
}
export interface StockTransferItem { variantId: string; sku: string; variantName: string; quantity: number }
export interface StockTransferDetail extends StockTransfer { reason: string | null; items: StockTransferItem[] }
