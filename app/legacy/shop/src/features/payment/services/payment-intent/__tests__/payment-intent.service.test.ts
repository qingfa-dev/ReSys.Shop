import { describe, it, expect } from 'vitest'
import { paymentIntentService } from '../payment-intent.service'

describe('PaymentIntentService', () => {
  describe('createPaymentIntent', () => {
    it('should create payment intent', async () => {
      const result = await paymentIntentService.createPaymentIntent(1000, 'USD')
      expect(result).toBeDefined()
    })
  })

  describe('getPaymentIntent', () => {
    it('should get payment intent', async () => {
      const result = await paymentIntentService.getPaymentIntent('pi-1')
      expect(result).toBeDefined()
    })
  })

  describe('confirmPayment', () => {
    it('should confirm payment', async () => {
      const result = await paymentIntentService.confirmPayment('pi-1', 'pm-1')
      expect(result).toBeDefined()
    })
  })

  describe('cancelPayment', () => {
    it('should cancel payment', async () => {
      const result = await paymentIntentService.cancelPayment('pi-1')
      expect(result).toBeDefined()
    })
  })
})