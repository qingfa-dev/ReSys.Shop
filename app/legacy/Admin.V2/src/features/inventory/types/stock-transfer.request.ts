export interface CreateStockTransferRequest {
  sourceLocationId: string
  destinationLocationId: string
  lineItems: { variantId: string; quantity: number }[]
  notes?: string | null
}
