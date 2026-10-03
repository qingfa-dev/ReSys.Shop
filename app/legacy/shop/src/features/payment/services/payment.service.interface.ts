import type { Result } from '@/core/models/result'
import type { PaymentIntent, Transaction } from '../types'

export interface IPaymentService {
  createPaymentIntent(amount: number, currency: string): Promise<Result<PaymentIntent>>
  getPaymentIntent(id: string): Promise<Result<PaymentIntent>>
  confirmPayment(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntent>>
  cancelPayment(paymentIntentId: string): Promise<Result<void>>
  getTransactionsByOrder(orderId: string): Promise<Result<Transaction[]>>
  refundTransaction(transactionId: string, amount?: number): Promise<Result<Transaction>>
}