import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { TransactionResponse } from '../../types/response'
import type { ITransactionRepository } from './transaction.repository.interface'

export class TransactionApiRepository extends BaseRepository implements ITransactionRepository {
  async getByOrderId(orderId: string): Promise<Result<TransactionResponse[]>> {
    return this.get<TransactionResponse[]>(`/api/storefront/payment/transactions`, { filter: `orderId:${orderId}` })
  }

  async getById<T = TransactionResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/api/storefront/payment/transactions/${id}`)
  }
}

export const transactionApiRepository = new TransactionApiRepository()