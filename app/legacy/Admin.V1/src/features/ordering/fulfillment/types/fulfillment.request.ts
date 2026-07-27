import type { FulfillmentParameters } from '../types/fulfillment.field'

export type CreateFulfillmentRequest = FulfillmentParameters

export interface RefundPaymentRequest {
  amountCents: number
  reason: string
}
