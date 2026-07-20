export const INVENTORY_ENDPOINTS = {
  STOCK: '/inventory/stock',
  STOCK_BY_PRODUCT: (productId: string) => `/inventory/stock/${productId}`,
  RESERVE: '/inventory/reserve',
  RELEASE: '/inventory/release',
  LOW_STOCK: '/inventory/low-stock',
} as const