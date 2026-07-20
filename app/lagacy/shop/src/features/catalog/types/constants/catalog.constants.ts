export const CATALOG_ENDPOINTS = {
  PRODUCTS: '/catalog/products',
  PRODUCT: (id: string) => `/catalog/products/${id}`,
  CATEGORIES: '/catalog/categories',
  CATEGORY: (id: string) => `/catalog/categories/${id}`,
  SEARCH: '/catalog/search',
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