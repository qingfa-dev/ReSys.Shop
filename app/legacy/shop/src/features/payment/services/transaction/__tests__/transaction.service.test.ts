import { describe, it, expect } from 'vitest'
import { transactionService } from '../transaction.service'

describe('TransactionService', () => {
  describe('getTransactionsByOrder', () => {
    it('should return transactions', async () => {
      const result = await transactionService.getTransactionsByOrder('order-1')
      expect(result).toBeDefined()
    })
  })

  describe('refundTransaction', () => {
    it('should refund transaction', async () => {
      const result = await transactionService.refundTransaction('txn-1')
      expect(result).toBeDefined()
    })
  })
})