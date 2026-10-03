export interface PaymentResponse {
  id: string
  orderId?: string | null
  orderNumber?: string | null
  paymentMethodId: string
  paymentMethodName?: string | null
  amount: number
  currency: string
  status: string
  authorizationCode?: string | null
  capturedAt?: string | null
  voidedAt?: string | null
  refundedAt?: string | null
  notes?: string | null
  createdAt: string
  updatedAt: string
}
