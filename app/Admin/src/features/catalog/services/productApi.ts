import { post, get, put, del, patch } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  ProductRequest,
  ProductListItem,
  ProductDetail,
} from '../types/product'
import {
  PRODUCT_FILTER_FIELDS,
  PRODUCT_SORT_FIELDS,
} from '../types/product'

export class ProductApi {
  static getProducts(params: QueryingParameters): Promise<PagedResult<ProductListItem>> {
    return getPaged<ProductListItem>('/api/admin/catalog/products', params, {
      allowedFilterFields: PRODUCT_FILTER_FIELDS,
      allowedSortFields: PRODUCT_SORT_FIELDS,
      allowedSearchFields: ['name', 'slug'],
    })
  }

  static getProduct(id: string): Promise<Result<ProductDetail>> {
    return get<Result<ProductDetail>>(`/api/admin/catalog/products/${id}`)
  }

  static createProduct(request: ProductRequest): Promise<Result<ProductDetail>> {
    return post<Result<ProductDetail>>('/api/admin/catalog/products', request)
  }

  static updateProduct(id: string, request: ProductRequest): Promise<Result<ProductDetail>> {
    return put<Result<ProductDetail>>(`/api/admin/catalog/products/${id}`, request)
  }

  static deleteProduct(id: string): Promise<Result<ProductListItem>> {
    return del<Result<ProductListItem>>(`/api/admin/catalog/products/${id}`)
  }

  static activateProduct(id: string): Promise<Result<ProductDetail>> {
    return patch<Result<ProductDetail>>(`/api/admin/catalog/products/${id}/activate`)
  }

  static discontinueProduct(id: string): Promise<Result<ProductDetail>> {
    return patch<Result<ProductDetail>>(`/api/admin/catalog/products/${id}/discontinue`)
  }
}
