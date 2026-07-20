export interface PaymentListItem {
  id: string
  orderId: string
  amount: number
  currency: string
  status: number
  methodName: string
  createdAtUtc: string
}

export interface PaymentDetail extends PaymentListItem {
  gatewayResponse: Record<string, unknown> | null
  transactions: PaymentTransaction[]
}

export interface PaymentTransaction {
  id: string
  type: string
  amount: number
  status: string
  gatewayTransactionId: string | null
  createdAtUtc: string
}
