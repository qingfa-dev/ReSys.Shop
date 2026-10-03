import { describe, it, expect } from 'vitest'
import { mockShippingRateRepository } from '../shipping-rate.mock.repository'

describe('ShippingRateRepository', () => {
  describe('getAll', () => {
    it('should return all shipping rates', async () => {
      const result = await mockShippingRateRepository.getAll('NY', 10)
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(4)
    })

    it('should accept destination and weight parameters', async () => {
      const result = await mockShippingRateRepository.getAll('CA', 5)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getById', () => {
    it('should return shipping rate by id', async () => {
      const result = await mockShippingRateRepository.getById('rate-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.carrier).toBe('FedEx')
      expect(result.data?.name).toBe('Ground')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockShippingRateRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockShippingRateRepository.getById('rate-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('calculateCost', () => {
    it('should calculate base cost', async () => {
      const result = await mockShippingRateRepository.calculateCost('rate-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBe(9.99)
    })

    it('should include distance cost when provided', async () => {
      const result = await mockShippingRateRepository.calculateCost('rate-1', 100)
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBeCloseTo(19.99)
    })

    it('should return error for non-existent rate', async () => {
      const result = await mockShippingRateRepository.calculateCost('invalid-id')
      expect(result.isFailure).toBe(true)
    })
  })

  describe('getEstimatedDelivery', () => {
    it('should return estimated delivery date', async () => {
      const result = await mockShippingRateRepository.getEstimatedDelivery('rate-1', 'NY')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBeDefined()
    })

    it('should return error for non-existent rate', async () => {
      const result = await mockShippingRateRepository.getEstimatedDelivery('invalid-id', 'NY')
      expect(result.isFailure).toBe(true)
    })
  })
})