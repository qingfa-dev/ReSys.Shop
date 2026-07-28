export const ORDER_ENDPOINTS = {
  CART: '/api/storefront/cart',
  CART_ITEM: (id: string) => `/api/storefront/cart/items/${id}`,
  CHECKOUT: '/ordering/checkout',
  ORDERS: '/api/storefront/orders',
  ORDER: (id: string) => `/api/storefront/orders/${id}`,
  ORDER_CANCEL: (id: string) => `/api/storefront/orders/${id}/cancel`,
  SHIPPING_METHODS: '/ordering/shipping-methods',
  PAYMENT_METHODS: '/ordering/payment-methods',
} as const

export const ORDER_STATUS = {
  PENDING: 'pending',
  PROCESSING: 'processing',
  SHIPPED: 'shipped',
  DELIVERED: 'delivered',
  CANCELLED: 'cancelled',
  REFUNDED: 'refunded',
} as const

export type OrderStatus = typeof ORDER_STATUS[keyof typeof ORDER_STATUS]

export const PAYMENT_METHODS = {
  CARD: 'card',
  PAYPAL: 'paypal',
  APPLEPAY: 'applepay',
  GOOGLEPAY: 'googlepay',
} as const

export type PaymentMethodType = typeof PAYMENT_METHODS[keyof typeof PAYMENT_METHODS]

export const DEFAULT_CURRENCY = 'USD'

export const TAX_RATE = 0.1