import { describe, it, expect } from 'vitest'
import { shipmentService } from '../shipment.service'

describe('ShipmentService', () => {
  describe('getShipment', () => {
    it('should return shipment', async () => {
      const result = await shipmentService.getShipment('TRACK123')
      expect(result).toBeDefined()
    })
  })

  describe('getShipmentsByOrder', () => {
    it('should return shipments by order', async () => {
      const result = await shipmentService.getShipmentsByOrder('order-1')
      expect(result).toBeDefined()
    })
  })
})