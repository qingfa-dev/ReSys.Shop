export interface AddToCartRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}

export interface CheckoutRequest {
  paymentIntentId: string
}

export interface ApplyCouponRequest {
  couponCode: string
}

export interface ShippingMethodRequest {
  addressId: string
}

export interface PaymentMethodRequest {
  methodId: string
}