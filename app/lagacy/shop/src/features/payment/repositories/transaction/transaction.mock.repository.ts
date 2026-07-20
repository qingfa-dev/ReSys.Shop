import type { TransactionResponse } from '../../types/response'
import type { ITransactionRepository } from './transaction.repository.interface'
import type { Result } from '@/core/models/result'

const initialTransactions: TransactionResponse[] = [
  { id: 'txn-1', orderId: 'order-1', amount: 1000, currency: 'USD', status: 'completed', paymentMethod: 'card', createdAt: '2024-01-01T00:00:00Z' },
  { id: 'txn-2', orderId: 'order-1', amount: -500, currency: 'USD', status: 'refunded', paymentMethod: 'card', createdAt: '2024-01-02T00:00:00Z', refundedAt: '2024-01-02T00:00:00Z', refundAmount: 500 },
]

const mockTransactions: TransactionResponse[] = JSON.parse(JSON.stringify(initialTransactions))

export class MockTransactionRepository implements ITransactionRepository {
  static reset() {
    mockTransactions.length = 0
    initialTransactions.forEach(t => mockTransactions.push({ ...t }))
  }

  async getByOrderId(orderId: string): Promise<Result<TransactionResponse[]>> {
    const transactions = mockTransactions.filter(t => t.orderId === orderId)
    return { isSuccess: true, isFailure: false, statusCode: 200, data: transactions }
  }

  async getById<T = TransactionResponse>(id: string): Promise<Result<T>> {
    const transaction = mockTransactions.find(t => t.id === id)
    if (!transaction) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Transaction not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: transaction as T }
  }

  async refund(transactionId: string, amount?: number): Promise<Result<TransactionResponse>> {
    const transaction = mockTransactions.find(t => t.id === transactionId)
    if (!transaction) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Transaction not found' }
    }
    const refundAmount = amount ?? transaction.amount
    const refund: TransactionResponse = {
      id: `txn-${Date.now()}`,
      orderId: transaction.orderId,
      amount: -refundAmount,
      currency: transaction.currency,
      status: 'refunded',
      paymentMethod: transaction.paymentMethod,
      createdAt: new Date().toISOString(),
      refundedAt: new Date().toISOString(),
      refundAmount: refundAmount,
    }
    mockTransactions.push(refund)
    return { isSuccess: true, isFailure: false, statusCode: 200, data: refund }
  }
}

export const mockTransactionRepository = new MockTransactionRepository()