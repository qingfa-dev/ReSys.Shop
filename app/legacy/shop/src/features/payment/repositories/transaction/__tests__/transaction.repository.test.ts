import { describe, it, expect, beforeEach } from 'vitest'
import { mockTransactionRepository, MockTransactionRepository } from '../transaction.mock.repository'

describe('TransactionRepository', () => {
  beforeEach(() => {
    MockTransactionRepository.reset()
  })

  describe('getByOrderId', () => {
    it('should return transactions for order', async () => {
      const result = await mockTransactionRepository.getByOrderId('order-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(2)
    })

    it('should return empty array for order with no transactions', async () => {
      const result = await mockTransactionRepository.getByOrderId('order-nonexistent')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(0)
    })
  })

  describe('getById', () => {
    it('should return transaction by id', async () => {
      const result = await mockTransactionRepository.getById('txn-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('txn-1')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockTransactionRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockTransactionRepository.getById('txn-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('refund', () => {
    it('should refund full amount by default', async () => {
      const result = await mockTransactionRepository.refund('txn-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.status).toBe('refunded')
    })

    it('should refund partial amount when specified', async () => {
      const result = await mockTransactionRepository.refund('txn-1', 250)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.amount).toBe(-250)
    })

    it('should return error for non-existent transaction', async () => {
      const result = await mockTransactionRepository.refund('invalid-id')
      expect(result.isFailure).toBe(true)
    })
  })
})