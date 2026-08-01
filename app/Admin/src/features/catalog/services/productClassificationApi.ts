import { getPaged } from '@/shared/api'
import { post, put } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'

export interface ClassificationAssignment {
  taxonId: string
  name: string
  prettyName: string | null
  position: number
  isAssigned: boolean
}

export interface ClassificationSyncItem {
  taxonId: string
  position: number
}

export class ProductClassificationApi {
  private static readonly BASE = `${CATALOG}/product-classifications`

  static getClassifications(productId: string): Promise<PagedResult<ClassificationAssignment>> {
    return getPaged<ClassificationAssignment>(`${ProductClassificationApi.BASE}?productId=${productId}`, {})
  }

  static syncClassifications(request: { productId: string; items: ClassificationSyncItem[] }): Promise<Result<void>> {
    return put<Result<void>>(`${ProductClassificationApi.BASE}/sync`, request)
  }

  static assignClassifications(request: { productId: string; items: ClassificationSyncItem[] }): Promise<Result<void>> {
    return post<Result<void>>(`${ProductClassificationApi.BASE}/assign`, request)
  }

  static revokeClassifications(request: { productId: string; items: ClassificationSyncItem[] }): Promise<Result<void>> {
    return post<Result<void>>(`${ProductClassificationApi.BASE}/revoke`, request)
  }
}
