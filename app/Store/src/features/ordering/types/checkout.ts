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
  returnUrl?: string
}

export interface PlaceOrderRequest {
  paymentIntentId?: string
}

export interface PlaceOrderResponse { id: string }
export interface PaymentIntentResponse { id: string; clientSecret: string; responseCode?: string }
