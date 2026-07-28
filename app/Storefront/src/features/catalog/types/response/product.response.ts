import type { Result, PagedResult } from '@/core/models/result'
import type { ProductImage } from '../schemas/product.schema'

export interface ProductResponse {
  id: string
  name: string
  slug: string
  description: string
  price: number
  compareAtPrice?: number
  images: (string | ProductImage)[]
  category: CategoryResponse
  tags: string[]
  variants?: ProductVariantResponse[]
  inventory: ProductInventoryResponse
  createdAt: string
  updatedAt: string
}

export interface CategoryResponse {
  id: string
  name: string
  slug: string
  parentId?: string
  image?: string
}

export interface ProductVariantResponse {
  id: string
  productId: string
  name: string
  sku: string
  price: number
  options: VariantOptionResponse[]
  inventory: ProductInventoryResponse
}

export interface VariantOptionResponse {
  name: string
  value: string
}

export interface ProductInventoryResponse {
  quantity: number
  trackQuantity: boolean
  allowBackorder: boolean
  lowStockThreshold?: number
}

export type ProductListResponse = PagedResult<ProductResponse>

export type ProductSingleResponse = Result<ProductResponse>

export type CategoryListResponse = PagedResult<CategoryResponse>

export type CategorySingleResponse = Result<CategoryResponse>