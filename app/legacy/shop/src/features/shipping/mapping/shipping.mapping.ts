import type { ShippingRate, Shipment, ShippingRateSchemaType, ShipmentSchemaType } from '../types'

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

export function toShipment(schema: ShipmentSchemaType): Shipment {
  return {
    id: schema.id,
    orderId: schema.orderId,
    status: schema.status,
    trackingNumber: schema.trackingNumber,
    trackingUrl: schema.trackingUrl,
    carrier: schema.carrier,
    estimatedDelivery: schema.estimatedDelivery,
  }
}

export function fromShipment(shipment: Shipment): ShipmentSchemaType {
  return ShipmentSchema.parse(shipment)
}

export function isShipmentDelivered(shipment: Shipment): boolean {
  return shipment.status === 'delivered'
}

export function isShipmentInTransit(shipment: Shipment): boolean {
  return shipment.status === 'in_transit' || shipment.status === 'label_created'
}

export function getShipmentStatusLabel(status: Shipment['status']): string {
  const labels: Record<Shipment['status'], string> = {
    pending: 'Pending',
    label_created: 'Label Created',
    in_transit: 'In Transit',
    delivered: 'Delivered',
    exception: 'Exception',
  }
  return labels[status]
}

export function formatShippingPrice(price: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(price)
}

import { ShippingRateSchema, ShipmentSchema } from '../types/schemas'