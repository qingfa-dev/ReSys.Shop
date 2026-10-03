import { describe, it, expect } from 'vitest'
import { paymentMethodService } from '../payment-method.service'

describe('PaymentMethodService', () => {
  describe('getPaymentMethods', () => {
    it('should return payment methods', async () => {
      const result = await paymentMethodService.getPaymentMethods()
      expect(result).toBeDefined()
    })
  })
})