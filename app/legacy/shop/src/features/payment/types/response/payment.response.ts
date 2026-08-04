import type { Result } from '@/core/models/result'
import type { PaymentIntentSchemaType, TransactionSchemaType } from '../schemas'

export interface PaymentIntentResponse extends PaymentIntentSchemaType {}
export interface TransactionResponse extends TransactionSchemaType {}

export interface CreatePaymentIntentResponse {
  paymentIntent: PaymentIntentResponse
  clientSecret: string
}

export interface ConfirmPaymentResponse {
  paymentIntent: PaymentIntentResponse
  confirmedAt: string
}

export interface CancelPaymentResponse {
  paymentIntent: PaymentIntentResponse
  cancelledAt: string
  reason?: string
}

export interface RefundTransactionResponse {
  transaction: TransactionResponse
  refundedAmount: number
  refundedAt: string
}

export interface GetTransactionsResponse {
  transactions: TransactionResponse[]
  totalCount: number
}

export type PaymentIntentSingleResponse = Result<PaymentIntentResponse>
export type TransactionListResponse = Result<TransactionResponse[]>