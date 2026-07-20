export interface CreateReturnRequest {
  orderId: string
  items: { orderItemId: string; quantity: number; reason: string }[]
  refundMethod?: 'original' | 'store_credit'
}

export interface CancelReturnRequest {
  returnId: string
  reason?: string
}

export interface GetReturnLabelsRequest {
  returnId: string
}

export interface GetReturnRequestsByOrderRequest {
  orderId: string
}