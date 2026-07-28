export interface AddToCartRequest {
  productId: string
  variantId?: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}

export interface CheckoutRequest {
  items: { productId: string; variantId?: string; quantity: number }[]
  shippingAddressId: string
  billingAddressId?: string
  shippingMethodId: string
  paymentMethodId: string
  couponCode?: string
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