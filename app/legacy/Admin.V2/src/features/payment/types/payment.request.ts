export interface CapturePaymentRequest {
  amount?: number
}

export interface VoidPaymentRequest {
  reason?: string
}

export interface RefundPaymentRequest {
  amount?: number
  reason?: string
}
