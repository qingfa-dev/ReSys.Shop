export interface CreatePaymentIntentRequest {
  amount: number
  currency?: string
  orderId?: string
  metadata?: Record<string, string>
}

export interface ConfirmPaymentRequest {
  paymentIntentId: string
  paymentMethodId: string
}

export interface CancelPaymentRequest {
  paymentIntentId: string
  reason?: string
}

export interface RefundTransactionRequest {
  transactionId: string
  amount?: number
  reason?: string
}

export interface GetTransactionsRequest {
  orderId?: string
  status?: 'pending' | 'completed' | 'failed' | 'refunded'
}