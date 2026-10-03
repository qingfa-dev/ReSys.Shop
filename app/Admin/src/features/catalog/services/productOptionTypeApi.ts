import { getPaged } from '@/shared/api'
import { post, put } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { OptionTypeAssignment, ProductOptionTypeAssignmentRequest } from '../types/productOptionType'

export class ProductOptionTypeApi {
  static getOptionTypes(productId: string, params: QueryingParameters = {}): Promise<PagedResult<OptionTypeAssignment>> {
    return getPaged<OptionTypeAssignment>(`/api/admin/catalog/product-option-types?productId=${productId}`, params)
  }

  static syncOptionTypes(request: ProductOptionTypeAssignmentRequest): Promise<Result<void>> {
    return put<Result<void>>('/api/admin/catalog/product-option-types/sync', request)
  }

  static assignOptionTypes(request: ProductOptionTypeAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>('/api/admin/catalog/product-option-types/assign', request)
  }

  static revokeOptionTypes(request: ProductOptionTypeAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>('/api/admin/catalog/product-option-types/revoke', request)
  }
}
