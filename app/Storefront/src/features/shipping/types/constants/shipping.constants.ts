export const SHIPPING_ENDPOINTS = {
  METHODS: '/shipping/methods',
  METHOD: (id: string) => `/shipping/methods/${id}`,
  RATES: '/api/storefront/shipping/rates',
} as const