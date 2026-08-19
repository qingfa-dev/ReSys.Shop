import type { Result } from '@/core/models/result'
import type { PaymentMethodResponse } from '../../types/response'

export interface IPaymentMethodRepository {
  getAll(): Promise<Result<PaymentMethodResponse[]>>
  getById<T = PaymentMethodResponse>(id: string): Promise<Result<T>>
}