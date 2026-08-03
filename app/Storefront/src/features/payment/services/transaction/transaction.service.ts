import type { ITransactionService } from './transaction.service.interface'
import type { Transaction } from '../../types'
import type { Result } from '@/core/models/result'

export class TransactionService implements ITransactionService {
  // MVP: dropped — no storefront API for payment transactions
  async getTransactionsByOrder(_orderId: string): Promise<Result<Transaction[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: [], errors: [] }
  }
}

export const transactionService = new TransactionService()