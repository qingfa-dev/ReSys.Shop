export interface StockMovement {
  id: string; stockItemId: string; action: string; quantity: number
  previousCountOnHand: number; reason: string | null; reference: string | null
  createdAtUtc: string; createdBy: string | null
}
