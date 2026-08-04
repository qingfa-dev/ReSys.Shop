import type { Result } from '@/core/models/result'
import type { Transaction } from '../../types'

export interface ITransactionService {
  getTransactionsByOrder(orderId: string): Promise<Result<Transaction[]>>
  refundTransaction(transactionId: string, amount?: number): Promise<Result<Transaction>>
}