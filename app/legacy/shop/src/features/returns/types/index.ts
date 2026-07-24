export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface ReturnRequest {
  id: string
  orderId: string
  status: 'pending' | 'approved' | 'rejected' | 'received' | 'refunded'
  items: { orderItemId: string; quantity: number; reason: string }[]
  refundAmount: number
  refundMethod: 'original' | 'store_credit'
  trackingNumber?: string
  createdAt: string
  updatedAt: string
}

export interface CreateReturnRequest {
  orderId: string
  items: { orderItemId: string; quantity: number; reason: string }[]
  refundMethod?: 'original' | 'store_credit'
}
