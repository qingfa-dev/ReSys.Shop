import { describe, it, expect } from 'vitest'
import { paymentIntentService } from '../payment-intent.service'

describe('PaymentIntentService', () => {
  describe('createPaymentIntent', () => {
    it('should create payment intent', async () => {
      const result = await paymentIntentService.createPaymentIntent({
        amount: 1000,
        currency: 'USD',
        orderId: 'order-1',
        paymentMethodId: 'pm-1',
      })
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
})
