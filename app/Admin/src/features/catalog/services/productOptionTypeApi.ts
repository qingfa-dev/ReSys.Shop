import { getPaged } from '@/shared/api'
import { post, put } from '@/shared/api/client'

import type { Result, PagedResult } from '@/shared/types'
import type { OptionTypeAssignment, ProductOptionTypeAssignmentRequest } from '../types/productOptionType'

export class ProductOptionTypeApi {
  private static readonly BASE = 'api/admin/catalog/product-option-types'

  static getOptionTypes(productId: string): Promise<PagedResult<OptionTypeAssignment>> {
    return getPaged<OptionTypeAssignment>(`${ProductOptionTypeApi.BASE}?productId=${productId}`, {})
  }

  static syncOptionTypes(request: ProductOptionTypeAssignmentRequest): Promise<Result<void>> {
    return put<Result<void>>(`${ProductOptionTypeApi.BASE}/sync`, request)
  }

  static assignOptionTypes(request: ProductOptionTypeAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.BASE}/assign`, request)
  }

  static revokeOptionTypes(request: ProductOptionTypeAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.BASE}/revoke`, request)
  }
}
