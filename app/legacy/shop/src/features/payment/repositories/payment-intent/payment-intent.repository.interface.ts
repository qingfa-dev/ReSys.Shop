import type { Result } from '@/core/models/result'
import type { PaymentIntentResponse } from '../../types/response'

export interface IPaymentIntentRepository {
  create(amount: number, currency?: string): Promise<Result<PaymentIntentResponse>>
  getById<T = PaymentIntentResponse>(id: string): Promise<Result<T>>
  confirm(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntentResponse>>
  cancel(paymentIntentId: string): Promise<Result<void>>
}