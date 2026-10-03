export interface TransferLineItem {
  variantId: string
  variantSku?: string | null
  quantity: number
  receivedQuantity: number
}

export interface StockTransferResponse {
  id: string
  reference: string
  sourceLocationId: string
  sourceLocationName?: string | null
  destinationLocationId: string
  destinationLocationName?: string | null
  status: string
  lineItems: TransferLineItem[]
  notes?: string | null
  createdAt: string
  updatedAt: string
}
