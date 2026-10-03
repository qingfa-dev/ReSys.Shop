import type { PaymentIntent, Transaction, PaymentIntentSchemaType, TransactionSchemaType } from '../types'

export function toPaymentIntent(schema: PaymentIntentSchemaType): PaymentIntent {
  return {
    id: schema.id,
    amount: schema.amount,
    currency: schema.currency,
    status: schema.status,
    clientSecret: schema.clientSecret,
    responseCode: schema.responseCode,
  }
}

export function fromPaymentIntent(intent: PaymentIntent): PaymentIntentSchemaType {
  return PaymentIntentSchema.parse(intent)
}

export function toTransaction(schema: TransactionSchemaType): Transaction {
  return {
    id: schema.id,
    orderId: schema.orderId,
    amount: schema.amount,
    currency: schema.currency,
    status: schema.status,
    paymentMethod: schema.paymentMethod,
    createdAt: schema.createdAt,
  }
}

export function fromTransaction(transaction: Transaction): TransactionSchemaType {
  return TransactionSchema.parse(transaction)
}

export function isPaymentSuccessful(intent: PaymentIntent): boolean {
  return intent.status === 'succeeded'
}

export function isPaymentPending(intent: PaymentIntent): boolean {
  return intent.status === 'pending' || intent.status === 'processing'
}

export function formatAmount(amount: number, currency: string): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
  }).format(amount)
}

import { PaymentIntentSchema, TransactionSchema } from '../types/schemas'