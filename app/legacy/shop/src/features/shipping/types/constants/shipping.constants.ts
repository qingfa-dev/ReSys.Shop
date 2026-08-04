export const SHIPPING_ENDPOINTS = {
  METHODS: '/shipping/methods',
  METHOD: (id: string) => `/shipping/methods/${id}`,
  RATES: '/shipping/rates',
  TRACK: (id: string) => `/shipping/track/${id}`,
  SHIPMENTS: '/shipping/shipments',
  SHIPMENT: (id: string) => `/shipping/shipments/${id}`,
} as const