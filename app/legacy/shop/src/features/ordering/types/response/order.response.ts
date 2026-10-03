import type { Result, PagedResult } from '@/core/models/result'

export interface CartItemResponse {
  id: string
  productId: string
  productName: string
  productImage: string
  variantId?: string
  variantName?: string
  quantity: number
  price: number
  compareAtPrice?: number
}

export interface CartResponse {
  id: string
  items: CartItemResponse[]
  subtotal: number
  tax: number
  shipping: number
  discount: number
  total: number
  currency: string
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
  firstName: string
  lastName: string
  address1: string
  address2?: string
  city: string
  state: string
  postalCode: string
  country: string
  phone?: string
  isDefault?: boolean
}

export interface ShippingMethodResponse {
  id: string
  name: string
  description: string
  price: number
  estimatedDays: number
}

export interface PaymentMethodResponse {
  id: string
  name: string
  type: string
  last4?: string
  brand?: string
  expiryMonth?: number
  expiryYear?: number
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