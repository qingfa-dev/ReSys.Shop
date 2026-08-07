import { get, post } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { ProductListItemSchema, ProductDetailSchema } from '../validations/product'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreProductListItemResponse, StoreProductDetailResponse, ProductQuery } from '../types'
import { toProductQueryParams } from '../types'

const validatedPagedList = PagedResultSchema(ProductListItemSchema)

export class ProductApi {
  private static readonly BASE = `${CATALOG}/products`

  static async getProducts(q: ProductQuery): Promise<PagedResult<StoreProductListItemResponse>> {
    const params = toProductQueryParams(q)
    const result = await getPaged<unknown>(this.BASE, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }

  static async getProductBySlug(slug: string): Promise<PagedResult<StoreProductDetailResponse>> {
    const data = await get<unknown>(`${this.BASE}/${slug}`)
    if (!data.isSuccess) return data as PagedResult<StoreProductDetailResponse>
    data.value = ProductDetailSchema.parse(data.value)
    return data as PagedResult<StoreProductDetailResponse>
  }

  static async getSimilar(productId: string, topK?: number): Promise<PagedResult<StoreProductListItemResponse>> {
    const params: Record<string, unknown> = { productId }
    if (topK) params.topK = topK
    const result = await getPaged<unknown>(`${this.BASE}/similar`, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }

  static async getRelated(productId: string, q: ProductQuery): Promise<PagedResult<StoreProductListItemResponse>> {
    const params: Record<string, unknown> = { productId, ...toProductQueryParams(q) }
    const result = await getPaged<unknown>(`${this.BASE}/related`, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }
}
