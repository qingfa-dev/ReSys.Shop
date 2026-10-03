import type { CheckoutState } from './order'

export interface CartLineItem {
  id: string
  variantId: string
  productId: string | null
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

// Summary: Shipping calculation metadata captured when the shipping cost was last applied.
export interface ShippingCalculationSummary {
  totalWeight: number
  shippingRateId: string | null
  cost: number
  isFreeShipping: boolean
}

// Row: A persisted adjustment (e.g. the applied shipping cost, future discounts).
export interface AdjustmentSummary {
  id: string
  label: string
  amount: number
  sourceType: string
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
  shippingCalculation: ShippingCalculationSummary | null
  adjustments: AdjustmentSummary[]
  items: CartLineItem[]
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
