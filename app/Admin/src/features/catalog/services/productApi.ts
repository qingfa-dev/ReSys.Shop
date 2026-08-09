import { post, get, put, del, patch } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type {
  ProductRequest,
  ProductListItem,
  ProductDetail,
  ProductQuery,
} from '../types/product'
import {
  toProductQueryParams,
  PRODUCT_FILTER_FIELDS,
  PRODUCT_SORT_FIELDS,
} from '../types/product'

export class ProductApi {
  private static readonly BASE = 'api/admin/catalog/products'

  static getProducts(query: ProductQuery): Promise<PagedResult<ProductListItem>> {
    return getPaged<ProductListItem>(ProductApi.BASE, toProductQueryParams(query), {
      allowedFilterFields: PRODUCT_FILTER_FIELDS,
      allowedSortFields: PRODUCT_SORT_FIELDS,
    })
  }

  static getProduct(id: string): Promise<Result<ProductDetail>> {
    return get<Result<ProductDetail>>(`${ProductApi.BASE}/${id}`)
  }

  static createProduct(request: ProductRequest): Promise<Result<ProductDetail>> {
    return post<Result<ProductDetail>>(ProductApi.BASE, request)
  }

  static updateProduct(id: string, request: ProductRequest): Promise<Result<ProductDetail>> {
    return put<Result<ProductDetail>>(`${ProductApi.BASE}/${id}`, request)
  }

  static deleteProduct(id: string): Promise<Result<ProductListItem>> {
    return del<Result<ProductListItem>>(`${ProductApi.BASE}/${id}`)
  }

  static activateProduct(id: string): Promise<Result<ProductDetail>> {
    return patch<Result<ProductDetail>>(`${ProductApi.BASE}/${id}/activate`)
  }

  static discontinueProduct(id: string): Promise<Result<ProductDetail>> {
    return patch<Result<ProductDetail>>(`${ProductApi.BASE}/${id}/discontinue`)
  }
}
