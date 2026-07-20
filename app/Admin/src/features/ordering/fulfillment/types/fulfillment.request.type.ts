import type { FulfillmentParameters } from '../schemas/fulfillment.schema'

export type CreateFulfillmentRequest = FulfillmentParameters

export interface RefundPaymentRequest {
  amountCents: number
  reason: string
}
