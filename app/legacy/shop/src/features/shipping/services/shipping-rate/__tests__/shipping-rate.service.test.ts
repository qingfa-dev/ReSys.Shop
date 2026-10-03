import { describe, it, expect } from 'vitest'
import { shippingRateService } from '../shipping-rate.service'

describe('ShippingRateService', () => {
  describe('getShippingRates', () => {
    it('should return shipping rates', async () => {
      const result = await shippingRateService.getShippingRates('10001', 5)
      expect(result).toBeDefined()
    })
  })

  describe('calculateShippingCost', () => {
    it('should calculate shipping cost', async () => {
      const result = await shippingRateService.calculateShippingCost('rate-1')
      expect(result).toBeDefined()
    })
  })

  describe('getEstimatedDelivery', () => {
    it('should return estimated delivery', async () => {
      const result = await shippingRateService.getEstimatedDelivery('rate-1', '10001')
      expect(result).toBeDefined()
    })
  })
})