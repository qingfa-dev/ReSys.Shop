import { describe, it, expect } from 'vitest'
import {
  toShippingRate,
  fromShippingRate,
  toShipment,
  fromShipment,
  isShipmentDelivered,
  isShipmentInTransit,
  getShipmentStatusLabel,
} from '../mapping/shipping.mapping'
import { ShippingRateSchema, ShipmentSchema } from '../types/schemas'

describe('Shipping Mapping', () => {
  describe('toShippingRate', () => {
    it('should convert schema to entity', () => {
      const schema = ShippingRateSchema.parse({
        id: 'rate-1',
        name: 'Standard',
        carrier: 'USPS',
        price: 5.99,
        estimatedDays: 7,
        trackingEnabled: true,
      })
      const result = toShippingRate(schema)
      expect(result.name).toBe('Standard')
      expect(result.price).toBe(5.99)
    })
  })

  describe('toShipment', () => {
    it('should convert schema to entity', () => {
      const schema = ShipmentSchema.parse({
        id: 'ship-1',
        orderId: 'order-1',
        status: 'in_transit',
      })
      const result = toShipment(schema)
      expect(result.id).toBe('ship-1')
      expect(result.status).toBe('in_transit')
    })
  })

  describe('isShipmentDelivered', () => {
    it('should return true for delivered status', () => {
      const shipment = { id: 'ship-1', orderId: 'order-1', status: 'delivered' as const }
      expect(isShipmentDelivered(shipment)).toBe(true)
    })

    it('should return false for in_transit status', () => {
      const shipment = { id: 'ship-1', orderId: 'order-1', status: 'in_transit' as const }
      expect(isShipmentDelivered(shipment)).toBe(false)
    })
  })

  describe('isShipmentInTransit', () => {
    it('should return true for in_transit status', () => {
      const shipment = { id: 'ship-1', orderId: 'order-1', status: 'in_transit' as const }
      expect(isShipmentInTransit(shipment)).toBe(true)
    })

    it('should return true for label_created status', () => {
      const shipment = { id: 'ship-1', orderId: 'order-1', status: 'label_created' as const }
      expect(isShipmentInTransit(shipment)).toBe(true)
    })

    it('should return false for delivered status', () => {
      const shipment = { id: 'ship-1', orderId: 'order-1', status: 'delivered' as const }
      expect(isShipmentInTransit(shipment)).toBe(false)
    })
  })

  describe('getShipmentStatusLabel', () => {
    it('should return correct labels', () => {
      expect(getShipmentStatusLabel('pending')).toBe('Pending')
      expect(getShipmentStatusLabel('label_created')).toBe('Label Created')
      expect(getShipmentStatusLabel('in_transit')).toBe('In Transit')
      expect(getShipmentStatusLabel('delivered')).toBe('Delivered')
      expect(getShipmentStatusLabel('exception')).toBe('Exception')
    })
  })
})