export interface Fulfillment {
  id: string
  shipmentId: string
  trackingNumber?: string
  state: string
  createdAtUtc: string
}
