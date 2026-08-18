import type { PaymentRecordState } from '@/features/payment/types/payment'

export interface UpdateCheckoutRequest {
  shipAddressId?: string
  billAddressId?: string
  currency?: string
  email?: string
}

export interface SelectShippingRateRequest {
  shippingMethodId: string
}

export interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId: string
  paymentMethodToken?: string
  returnUrl?: string
  cancelUrl?: string
}

export interface PlaceOrderRequest {
  paymentIntentId?: string
}

export interface PlaceOrderResponse { id: string }
export interface PaymentIntentResponse {
  id: string
  clientSecret?: string | null
  responseCode?: string | null
  checkoutUrl?: string | null
  state?: PaymentRecordState | null
}
