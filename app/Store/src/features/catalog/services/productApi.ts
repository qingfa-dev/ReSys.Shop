import { z } from 'zod'
import { get, HttpError } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { PRODUCT_SORT_FIELDS, PRODUCT_SEARCH_FIELDS, PRODUCT_FILTER_FIELDS } from '@/shared/constants/product'
import { ProductListItemSchema, ProductDetailSchema } from '../validations/product'
import { PagedResultSchema } from '@/shared/validations/result'
import { failure } from '@/shared/types/result'
import type { PagedResult, Result } from '@/shared/types'
import type { StoreProductListItemResponse, StoreProductDetailResponse, ProductQuery } from '../types'
import { toProductQueryParams } from '../types'

// Validate: Paged result schema for product list items — reused across all list endpoints
const validatedPagedList = PagedResultSchema(ProductListItemSchema)
// Validate: Similar products schema extends list item with similarity score
const SimilarProductSchema = ProductListItemSchema.extend({ similarityScore: z.number() })
const validatedSimilarList = PagedResultSchema(SimilarProductSchema)

export class ProductApi {
  private static readonly BASE = `${CATALOG}/products`

  static async getProducts(q: ProductQuery): Promise<PagedResult<StoreProductListItemResponse>> {
    // Call: Catalog API — paginated product list with filters, sort, search
    const params = toProductQueryParams(q)
    const result = await getPaged<unknown>(this.BASE, params, {
      allowedSortFields: [...PRODUCT_SORT_FIELDS],
      allowedSearchFields: [...PRODUCT_SEARCH_FIELDS],
      allowedFilterFields: [...PRODUCT_FILTER_FIELDS],
    })
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    // Validate: Ensure API response matches ProductListItem schema
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }

  static async getProductBySlug(slug: string): Promise<Result<StoreProductDetailResponse>> {
    try {
      // Call: Catalog API — single product detail by SEO-friendly slug
      const data = await get<Result<StoreProductDetailResponse>>(`${this.BASE}/${slug}`)
      if (!data.isSuccess) return data
      // Validate: Ensure API response matches ProductDetail schema
      data.value = ProductDetailSchema.parse(data.value)
      return data
    } catch (e) {
      // Catch: Zod validation failures — return structured failure instead of throwing
      if (e instanceof z.ZodError) {
        return failure<StoreProductDetailResponse>(`Response validation failed: ${e.message}`, 500)
      }
      // Catch: HTTP errors from interceptor — return structured failure
      if (e instanceof HttpError) {
        return failure<StoreProductDetailResponse>(e.errors[0]?.message ?? 'Request failed', e.statusCode)
      }
      // Catch: Network and unexpected errors
      return failure<StoreProductDetailResponse>(e instanceof Error ? e.message : 'Unexpected error', 500)
    }
  }

  static async getSimilar(productId: string, topK?: number): Promise<PagedResult<StoreProductListItemResponse & { similarityScore: number }>> {
    // Call: Catalog API — AI-powered similar product recommendations
    const params: Record<string, unknown> = { productId }
    if (topK) params.topK = topK
    const result = await getPaged<unknown>(`${this.BASE}/similar`, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse & { similarityScore: number }>
    // Validate: Ensure API response matches SimilarProduct schema with similarity score
    const parsed = validatedSimilarList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse & { similarityScore: number }>
  }

  static async getRelated(productId: string, q: ProductQuery): Promise<PagedResult<StoreProductListItemResponse>> {
    // Call: Catalog API — paginated related products with filters
    const params: Record<string, unknown> = { productId, ...toProductQueryParams(q) }
    const result = await getPaged<unknown>(`${this.BASE}/related`, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    // Validate: Ensure API response matches ProductListItem schema
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }
}
