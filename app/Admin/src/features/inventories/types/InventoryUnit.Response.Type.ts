export type ReservationState = 'Reserved' | 'Fulfilled' | 'Released' | 'Expired'
export interface InventoryUnit {
  id: string; stockItemId: string; sku: string; serialNumber: string | null
  state: ReservationState; orderId: string | null; shipmentId: string | null; createdAtUtc: string
}
