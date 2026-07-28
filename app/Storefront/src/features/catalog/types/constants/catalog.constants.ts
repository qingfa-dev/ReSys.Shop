export const CATALOG_ENDPOINTS = {
  PRODUCTS: '/api/storefront/products',
  PRODUCT: (slug: string) => `/api/storefront/products/${slug}`,
  CATEGORIES: '/api/storefront/taxonomies',
  CATEGORY: (id: string) => `/api/storefront/taxonomies/${id}`,
  SEARCH: '/api/storefront/products?search=',
} as const

export const PRODUCT_SORT_OPTIONS = {
  NEWEST: 'newest',
  PRICE_ASC: 'price-asc',
  PRICE_DESC: 'price-desc',
  POPULAR: 'popular',
} as const

export type ProductSortOption = typeof PRODUCT_SORT_OPTIONS[keyof typeof PRODUCT_SORT_OPTIONS]

export const DEFAULT_PAGE_SIZE = 20

export const MAX_PAGE_SIZE = 100