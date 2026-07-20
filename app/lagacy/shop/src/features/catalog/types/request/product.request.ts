export interface ProductFilterRequest {
  category?: string
  priceMin?: number
  priceMax?: number
  tags?: string[]
  inStock?: boolean
}

export interface ProductSortRequest {
  sortBy?: 'newest' | 'price-asc' | 'price-desc' | 'popular'
}

export interface ProductSearchRequest {
  query?: string
  searchFields?: string[]
}