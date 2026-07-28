import { transactionApiRepository } from '../../repositories/transaction/transaction.api'
import { mockTransactionRepository } from '../../repositories/transaction/transaction.mock.repository'
import type { ITransactionService } from './transaction.service.interface'
import type { Transaction } from '../../types'
import type { Result } from '@/core/models/result'
import { toTransaction } from '../../mapping'
import { succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class TransactionService implements ITransactionService {
  private readonly transactionRepository = USE_MOCK ? mockTransactionRepository : transactionApiRepository

  async getTransactionsByOrder(orderId: string): Promise<Result<Transaction[]>> {
    const response = await this.transactionRepository.getByOrderId(orderId)
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get transactions', response.statusCode, response.errors)
    }
    return succeed(response.data!.map(toTransaction), response.statusCode)
  }
}

export const transactionService = new TransactionService()