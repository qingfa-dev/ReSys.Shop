import type { Result } from '@/core/models/result'
import type { PaymentIntent } from '../../types'

export interface CreatePaymentIntentParams {
  amount: number
  currency: string
  orderId: string // the draft cart/order id
  paymentMethodId: string
}

export interface IPaymentIntentService {
  createPaymentIntent(params: CreatePaymentIntentParams): Promise<Result<PaymentIntent>>
  getPaymentIntent(id: string): Promise<Result<PaymentIntent>>
  confirmPayment(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntent>>
}