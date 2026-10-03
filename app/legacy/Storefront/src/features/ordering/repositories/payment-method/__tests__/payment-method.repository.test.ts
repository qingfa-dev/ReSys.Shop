import { describe, it, expect } from 'vitest'
import { mockPaymentMethodRepository } from '../payment-method.mock.repository'

describe('PaymentMethodRepository', () => {
  describe('getAll', () => {
    it('should return payment methods', async () => {
      const result = await mockPaymentMethodRepository.getAll()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.length).toBe(3)
    })
  })

  describe('getById', () => {
    it('should return payment method by id', async () => {
      const result = await mockPaymentMethodRepository.getById('pm-1')
      expect(result.isSuccess).toBe(true)
    })
  })
})