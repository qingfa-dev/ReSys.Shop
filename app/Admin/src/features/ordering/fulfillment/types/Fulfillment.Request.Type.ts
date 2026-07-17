import type { FulfillmentParameters } from '../schemas/Fulfillment.Schema'

export type CreateFulfillmentRequest = FulfillmentParameters

export interface RefundPaymentRequest {
  amountCents: number
  reason: string
}
