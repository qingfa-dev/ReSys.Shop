import type { Result } from '@/core/models/result'
import type { PaymentIntent, Transaction } from '../types'
import type { CreatePaymentIntentParams } from './payment-intent/payment-intent.service.interface'

export interface IPaymentService {
  createPaymentIntent(params: CreatePaymentIntentParams): Promise<Result<PaymentIntent>>
  getPaymentIntent(id: string): Promise<Result<PaymentIntent>>
  confirmPayment(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntent>>
  getTransactionsByOrder(orderId: string): Promise<Result<Transaction[]>>
}