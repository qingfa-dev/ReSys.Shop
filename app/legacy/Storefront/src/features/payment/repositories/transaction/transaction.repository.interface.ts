import type { Result } from '@/core/models/result'
import type { TransactionResponse } from '../../types/response'

export interface ITransactionRepository {
  getByOrderId(orderId: string): Promise<Result<TransactionResponse[]>>
  getById<T = TransactionResponse>(id: string): Promise<Result<T>>
}