import { describe, it, expect, beforeEach } from 'vitest'
import { mockPaymentIntentRepository, MockPaymentIntentRepository } from '../payment-intent.mock.repository'

describe('PaymentIntentRepository', () => {
  beforeEach(() => {
    MockPaymentIntentRepository.reset()
  })

  describe('create', () => {
    it('should create payment intent with given currency', async () => {
      const result = await mockPaymentIntentRepository.create({
        amount: 1000,
        currency: 'USD',
        orderId: 'order-1',
        paymentMethodId: 'pm-1',
      })
      expect(result.isSuccess).toBe(true)
      expect(result.data?.amount).toBe(1000)
      expect(result.data?.currency).toBe('USD')
      expect(result.data?.status).toBe('pending')
    })

    it('should create payment intent with custom currency', async () => {
      const result = await mockPaymentIntentRepository.create({
        amount: 500,
        currency: 'EUR',
        orderId: 'order-1',
        paymentMethodId: 'pm-1',
      })
      expect(result.isSuccess).toBe(true)
      expect(result.data?.currency).toBe('EUR')
    })
  })

  describe('getById', () => {
    it('should return payment intent by id', async () => {
      const createResult = await mockPaymentIntentRepository.create({
        amount: 1000,
        currency: 'USD',
        orderId: 'order-1',
        paymentMethodId: 'pm-1',
      })
      const result = await mockPaymentIntentRepository.getById(createResult.data!.id)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe(createResult.data?.id)
    })

    it('should return error for non-existent id', async () => {
      const result = await mockPaymentIntentRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const createResult = await mockPaymentIntentRepository.create({
        amount: 1000,
        currency: 'USD',
        orderId: 'order-1',
        paymentMethodId: 'pm-1',
      })
      const result = await mockPaymentIntentRepository.getById(createResult.data!.id)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('confirm', () => {
    it('should confirm payment intent', async () => {
      const createResult = await mockPaymentIntentRepository.create({
        amount: 1000,
        currency: 'USD',
        orderId: 'order-1',
        paymentMethodId: 'pm-1',
      })
      const result = await mockPaymentIntentRepository.confirm(createResult.data!.id, 'pm-123')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.status).toBe('succeeded')
    })

    it('should return error for non-existent intent', async () => {
      const result = await mockPaymentIntentRepository.confirm('invalid-id', 'pm-123')
      expect(result.isFailure).toBe(true)
    })
  })
})
