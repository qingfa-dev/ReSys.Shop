export const PAYMENT_ENDPOINTS = {
  METHODS: '/api/storefront/payment/methods',
  METHOD: (id: string) => `/api/storefront/payment/methods/${id}`,
} as const