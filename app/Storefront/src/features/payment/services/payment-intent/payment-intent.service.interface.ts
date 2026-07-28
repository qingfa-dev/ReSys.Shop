import type { Result } from '@/core/models/result'
import type { PaymentIntent } from '../../types'

export interface IPaymentIntentService {
  createPaymentIntent(amount: number, currency: string): Promise<Result<PaymentIntent>>
  getPaymentIntent(id: string): Promise<Result<PaymentIntent>>
  confirmPayment(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntent>>
}