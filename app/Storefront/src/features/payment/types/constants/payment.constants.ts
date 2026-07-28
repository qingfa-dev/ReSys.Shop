export const PAYMENT_ENDPOINTS = {
  METHODS: '/api/storefront/payment/methods',
  METHOD: (id: string) => `/api/storefront/payment/methods/${id}`,
  PROCESS: '/api/storefront/payment/process',
  VERIFY: '/api/storefront/payment/verify',
} as const