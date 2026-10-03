export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface PaymentIntent {
  id: string
  amount: number
  currency: string
  status: 'pending' | 'processing' | 'succeeded' | 'failed'
  clientSecret?: string
}

export interface Transaction {
  id: string
  orderId: string
  amount: number
  currency: string
  status: 'pending' | 'completed' | 'failed' | 'refunded'
  paymentMethod: string
  createdAt: string
}
