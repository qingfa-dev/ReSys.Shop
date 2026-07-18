export interface CapturePaymentRequest {
  amount?: number
}

export interface RefundPaymentRequest {
  amount: number
  reason?: string
}
