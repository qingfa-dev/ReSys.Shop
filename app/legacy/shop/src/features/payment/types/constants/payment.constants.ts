export const PAYMENT_ENDPOINTS = {
  METHODS: '/payment/methods',
  METHOD: (id: string) => `/payment/methods/${id}`,
  PROCESS: '/payment/process',
  VERIFY: '/payment/verify',
  REFUND: (id: string) => `/payment/${id}/refund`,
} as const