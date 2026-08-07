import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type {
  StoreProductListItemResponse,
  StoreProductDetailResponse,
} from '../types/product'

export interface ProductFilterParams {
  searchQuery?: string
  taxonIds?: string[]
  optionValueIds?: string[]
  minPrice?: number | null
  maxPrice?: number | null
}

export function getPagedProducts(params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(ENDPOINTS.products, params)
}

export function buildProductFilterUrl(filters: ProductFilterParams): string {
  const params = new URLSearchParams()
  if (filters.searchQuery) params.append('search', filters.searchQuery)
  filters.taxonIds?.forEach(id => params.append('taxonId', id))
  filters.optionValueIds?.forEach(id => params.append('optionValueId', id))
  if (filters.minPrice != null) params.append('minPrice', String(filters.minPrice))
  if (filters.maxPrice != null) params.append('maxPrice', String(filters.maxPrice))
  const qs = params.toString()
  return qs ? `${ENDPOINTS.products}?${qs}` : ENDPOINTS.products
}

export function getProductBySlug(slug: string): Promise<Result<StoreProductDetailResponse>> {
  return get<Result<StoreProductDetailResponse>>(ENDPOINTS.productBySlug(slug))
}

export function getSimilarProducts(productId: string, topK = 20): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.productSimilar}?productId=${productId}&topK=${topK}`,
    { pageNumber: 1, pageSize: topK },
  )
}

export function getRelatedProducts(productId: string, params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.productRelated}?productId=${productId}`,
    params,
  )
}
