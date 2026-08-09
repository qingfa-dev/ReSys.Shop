import { getPaged } from '@/shared/api'
import { post, put } from '@/shared/api/client'

import type { Result, PagedResult } from '@/shared/types'
import type { ClassificationAssignment, ProductClassificationAssignmentRequest } from '../types/productClassification'

export class ProductClassificationApi {
  private static readonly BASE = 'api/admin/catalog/product-classifications'

  static getClassifications(productId: string): Promise<PagedResult<ClassificationAssignment>> {
    return getPaged<ClassificationAssignment>(`${ProductClassificationApi.BASE}?productId=${productId}`, {})
  }

  static syncClassifications(request: ProductClassificationAssignmentRequest): Promise<Result<void>> {
    return put<Result<void>>(`${ProductClassificationApi.BASE}/sync`, request)
  }

  static assignClassifications(request: ProductClassificationAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>(`${ProductClassificationApi.BASE}/assign`, request)
  }

  static revokeClassifications(request: ProductClassificationAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>(`${ProductClassificationApi.BASE}/revoke`, request)
  }
}
