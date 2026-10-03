import { describe, it, expect } from 'vitest'
import { mockShipmentRepository } from '../shipment.mock.repository'

describe('ShipmentRepository', () => {
  describe('getByTrackingNumber', () => {
    it('should return shipment by tracking number', async () => {
      const result = await mockShipmentRepository.getByTrackingNumber('TRK123456789')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.trackingNumber).toBe('TRK123456789')
      expect(result.data?.status).toBe('delivered')
    })

    it('should return error for non-existent tracking number', async () => {
      const result = await mockShipmentRepository.getByTrackingNumber('INVALID')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })

  describe('getByOrderId', () => {
    it('should return shipments for order', async () => {
      const result = await mockShipmentRepository.getByOrderId('order-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(1)
    })

    it('should return empty array for order with no shipments', async () => {
      const result = await mockShipmentRepository.getByOrderId('order-nonexistent')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(0)
    })
  })

  describe('getById', () => {
    it('should return shipment by id', async () => {
      const result = await mockShipmentRepository.getById('ship-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('ship-1')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockShipmentRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockShipmentRepository.getById('ship-1')
      expect(result.isSuccess).toBe(true)
    })
  })
})