import type { Fulfillment } from '../types/fulfillment.response.type'

export type FulfillmentModel = Fulfillment

export function mapFulfillment(dto: Fulfillment): FulfillmentModel {
  return { ...dto }
}
