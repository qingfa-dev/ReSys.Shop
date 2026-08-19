import { getPaged } from '@/shared/api'
import { post, put } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { ClassificationAssignment, ProductClassificationAssignmentRequest } from '../types/productClassification'

export class ProductClassificationApi {
  static getClassifications(productId: string, params: QueryingParameters = {}): Promise<PagedResult<ClassificationAssignment>> {
    return getPaged<ClassificationAssignment>(`/api/admin/catalog/product-classifications?productId=${productId}`, params)
  }

  static syncClassifications(request: ProductClassificationAssignmentRequest): Promise<Result<void>> {
    return put<Result<void>>('/api/admin/catalog/product-classifications/sync', request)
  }

  static assignClassifications(request: ProductClassificationAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>('/api/admin/catalog/product-classifications/assign', request)
  }

  static revokeClassifications(request: ProductClassificationAssignmentRequest): Promise<Result<void>> {
    return post<Result<void>>('/api/admin/catalog/product-classifications/revoke', request)
  }
}
