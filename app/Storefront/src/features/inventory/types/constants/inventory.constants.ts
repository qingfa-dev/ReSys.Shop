export const INVENTORY_ENDPOINTS = {
  AVAILABILITY: (variantId: string) => `/api/storefront/availability/${variantId}`,
  CART_RESERVE: '/api/storefront/cart/reserve',
} as const