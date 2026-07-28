export const INVENTORY_ENDPOINTS = {
  STOCK: '/api/storefront/inventory/stock',
  STOCK_BY_PRODUCT: (productId: string) => `/api/storefront/inventory/stock/${productId}`,
  RESERVE: '/api/storefront/inventory/reserve',
  RELEASE: '/api/storefront/inventory/release',
  LOW_STOCK: '/api/storefront/inventory/low-stock',
} as const