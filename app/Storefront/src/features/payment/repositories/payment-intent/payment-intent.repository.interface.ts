import type { Result } from '@/core/models/result'
import type { PaymentIntentResponse } from '../../types/response'
import type { CreatePaymentIntentParams } from '../../services/payment-intent/payment-intent.service.interface'

export interface IPaymentIntentRepository {
  create(params: CreatePaymentIntentParams): Promise<Result<PaymentIntentResponse>>
  getById<T = PaymentIntentResponse>(id: string): Promise<Result<T>>
  confirm(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntentResponse>>
}