export interface UpdateCheckoutRequest {
  shipAddressId: string
  currency: string
  email: string
}

export interface SelectShippingRateRequest {
  shippingMethodId: string
}

export interface CreatePaymentIntentRequest {
  orderId: string
  amount: number
  currency: string
  paymentMethodId: string
}

export interface PaymentIntentResponse {
  id: string
  clientSecret: string
  responseCode: string | null
}

export interface PlaceOrderRequest {
  paymentIntentId: string
}

export interface PlaceOrderResponse {
  id: string
}
