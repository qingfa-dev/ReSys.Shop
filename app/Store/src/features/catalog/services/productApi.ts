import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type {
  StoreProductListItemResponse,
  StoreProductDetailResponse,
} from '../types/product'

export function getPagedProducts(params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(ENDPOINTS.products, params)
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
