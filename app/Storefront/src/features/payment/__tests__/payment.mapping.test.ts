import { describe, it, expect } from 'vitest'
import {
  toPaymentIntent,
  fromPaymentIntent,
  toTransaction,
  fromTransaction,
  isPaymentSuccessful,
  isPaymentPending,
  formatAmount,
} from '../mapping/payment.mapping'
import { PaymentIntentSchema, TransactionSchema } from '../types/schemas'

describe('Payment Mapping', () => {
  describe('toPaymentIntent', () => {
    it('should convert schema to entity', () => {
      const schema = PaymentIntentSchema.parse({
        id: 'pi-1',
        amount: 100,
        currency: 'USD',
        status: 'succeeded',
      })
      const result = toPaymentIntent(schema)
      expect(result.id).toBe('pi-1')
      expect(result.amount).toBe(100)
    })
  })

  describe('isPaymentSuccessful', () => {
    it('should return true for succeeded status', () => {
      const intent = { id: 'pi-1', amount: 100, currency: 'USD', status: 'succeeded' as const }
      expect(isPaymentSuccessful(intent)).toBe(true)
    })

    it('should return false for pending status', () => {
      const intent = { id: 'pi-1', amount: 100, currency: 'USD', status: 'pending' as const }
      expect(isPaymentSuccessful(intent)).toBe(false)
    })
  })

  describe('isPaymentPending', () => {
    it('should return true for pending status', () => {
      const intent = { id: 'pi-1', amount: 100, currency: 'USD', status: 'pending' as const }
      expect(isPaymentPending(intent)).toBe(true)
    })

    it('should return true for processing status', () => {
      const intent = { id: 'pi-1', amount: 100, currency: 'USD', status: 'processing' as const }
      expect(isPaymentPending(intent)).toBe(true)
    })

    it('should return false for succeeded status', () => {
      const intent = { id: 'pi-1', amount: 100, currency: 'USD', status: 'succeeded' as const }
      expect(isPaymentPending(intent)).toBe(false)
    })
  })

  describe('formatAmount', () => {
    it('should format amount with currency', () => {
      const result = formatAmount(100.50, 'USD')
      expect(result).toBe('$100.50')
    })
  })

  describe('toTransaction', () => {
    it('should convert schema to entity', () => {
      const schema = TransactionSchema.parse({
        id: 'txn-1',
        orderId: 'order-1',
        amount: 50,
        currency: 'USD',
        status: 'completed',
        paymentMethod: 'card',
        createdAt: '2026-01-01T00:00:00Z',
      })
      const result = toTransaction(schema)
      expect(result.id).toBe('txn-1')
      expect(result.amount).toBe(50)
    })
  })
})