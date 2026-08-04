import type { ShippingRate, ShippingRateSchemaType } from '../types'

export function toShippingRate(schema: ShippingRateSchemaType): ShippingRate {
  return {
    id: schema.id,
    name: schema.name,
    carrier: schema.carrier,
    price: schema.price,
    estimatedDays: schema.estimatedDays,
    trackingEnabled: schema.trackingEnabled,
  }
}

export function fromShippingRate(rate: ShippingRate): ShippingRateSchemaType {
  return ShippingRateSchema.parse(rate)
}

export function formatShippingPrice(price: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(price)
}

import { ShippingRateSchema } from '../types/schemas'