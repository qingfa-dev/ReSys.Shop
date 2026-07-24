import { describe, it, expect } from 'vitest'
import { shippingMethodService } from '../shipping-method.service'

describe('ShippingMethodService', () => {
  describe('getShippingMethods', () => {
    it('should return shipping methods', async () => {
      const result = await shippingMethodService.getShippingMethods()
      expect(result).toBeDefined()
    })
  })
})