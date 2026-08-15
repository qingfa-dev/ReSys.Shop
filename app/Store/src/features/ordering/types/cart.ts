import type { CheckoutState } from './order'

export interface CartLineItem {
  id: string
  variantId: string
  variantName: string
  sku: string
  productName: string | null
  productImageUrl: string | null
  quantity: number
  price: number
  total: number
}

export interface ShippingAdjustmentSummary {
  id: string
  label: string
  amount: number
  shippingMethodId: string | null
}

export interface CartResponse {
  id: string
  itemTotal: number
  total: number
  currency: string
  itemCount: number
  checkoutState: CheckoutState
  shippingMethodId: string | null
  shipAddressId: string | null
  email: string | null
  shipmentTotal: number
  adjustmentTotal: number
  shippingAdjustment: ShippingAdjustmentSummary | null
  items: CartLineItem[]
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
