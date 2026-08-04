export const SHIPPING_ENDPOINTS = {
  METHODS: '/api/storefront/shipping/methods',
  METHOD: (id: string) => `/api/storefront/shipping/methods/${id}`,
  RATES: '/api/storefront/shipping/rates',
} as const