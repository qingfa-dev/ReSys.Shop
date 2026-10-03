import type { Result, PagedResult } from '@/core/models/result'

export interface CartItemResponse {
  variantId: string
  variantName?: string
  sku?: string
  productName?: string
  productImage?: string | null
  quantity: number
  price: number
  compareAtPrice?: number | null
}

export interface CartResponse {
  id: string
  items: CartItemResponse[]
  itemTotal: number      // backend field — maps to subtotal
  total: number
  currency: string
  itemCount: number
  checkoutState: string
  tax?: number
  shipping?: number
  discount?: number
}

export interface OrderItemResponse {
  id: string
  productId: string
  productName: string
  productImage: string
  variantName?: string
  quantity: number
  price: number
}

export interface OrderResponse {
  id: string
  orderNumber: string
  status: string
  items: OrderItemResponse[]
  shippingAddress: AddressResponse
  billingAddress: AddressResponse
  subtotal: number
  tax: number
  shipping: number
  discount: number
  total: number
  currency: string
  createdAt: string
  updatedAt: string
  trackingNumber?: string
}

export interface AddressResponse {
  id: string
  userId?: string
  addressType?: string
  firstName: string
  lastName?: string
  address1: string
  address2?: string
  city: string
  zipCode?: string
  phone?: string
  label?: string
  isDefault?: boolean
  countryName?: string
  stateProvince?: string
  countryCode?: string
  stateCode?: string
}

export interface ShippingMethodResponse {
  id: string
  name: string
  adminName?: string
  code?: string
  calculatorType?: string
  position?: number
}

export interface PaymentMethodResponse {
  id: string
  name: string
  code?: string
  description?: string
  providerKey?: string
}

export type CartSingleResponse = Result<CartResponse>
export type OrderSingleResponse = Result<OrderResponse>
export type OrderListResponse = PagedResult<OrderResponse>
export type ShippingMethodsResponse = PagedResult<ShippingMethodResponse>
export type PaymentMethodsResponse = PagedResult<PaymentMethodResponse>
export type AddressSingleResponse = Result<AddressResponse>
export type AddressListResponse = Result<AddressResponse[]>
export type ShippingMethodSingleResponse = Result<ShippingMethodResponse>
export type PaymentMethodSingleResponse = Result<PaymentMethodResponse>