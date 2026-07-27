export interface StockMovement {
  id: string; stockItemId: string; quantity: number
  type: number; reason: string | null; reference: string | null
  createdAtUtc: string; createdBy: string | null
}
