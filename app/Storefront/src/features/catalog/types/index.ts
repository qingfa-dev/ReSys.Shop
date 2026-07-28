import type { Product, ProductImage, ProductInventory } from './schemas/product.schema'
import type { Category } from './schemas/product.schema'

export * from './response'
export * from './request'
export * from './params'
export * from './schemas'
export * from './constants'

export type { Product, Category, ProductImage, ProductInventory } from './schemas/product.schema'
export type { ProductVariantSchema, ProductInventorySchema } from './schemas/product.schema'

export interface ProductFilter {
  category?: string
  priceMin?: number
  priceMax?: number
  tags?: string[]
  inStock?: boolean
  sortBy?: 'newest' | 'price-asc' | 'price-desc' | 'popular'
  size?: string
  color?: string
  brand?: string
}

export interface PaginatedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ProductColor {
  id: string
  name: string
  hex: string
}

export interface ProductSize {
  id: string
  name: string
  stock: number
}

export interface SizeChartRow {
  size: string
  chest: string
  length: string
  sleeves: string
}

export interface ProductReview {
  author: string
  title: string
  text: string
  rating: number
}

export interface ProductDetail extends Product {
  brand: string
  rating: number
  reviews: number
  longDescription: string
  colors?: ProductColor[]
  sizes?: ProductSize[]
  sizeChart?: SizeChartRow[]
  reviewsList?: ProductReview[]
  inStock: boolean
  material?: string
  careInstructions?: string[]
  dimensions?: string
  weight?: string
  origin?: string
}