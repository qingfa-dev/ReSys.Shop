import { getPaged } from '@/shared/api'
import { post, put } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'

export interface OptionTypeAssignment {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

export interface OptionTypeSyncItem {
  optionTypeId: string
  position: number
}

export class ProductOptionTypeApi {
  private static readonly BASE = `${CATALOG}/product-option-types`

  static getOptionTypes(productId: string): Promise<PagedResult<OptionTypeAssignment>> {
    return getPaged<OptionTypeAssignment>(`${ProductOptionTypeApi.BASE}?productId=${productId}`, {})
  }

  static syncOptionTypes(request: { productId: string; items: OptionTypeSyncItem[] }): Promise<Result<void>> {
    return put<Result<void>>(`${ProductOptionTypeApi.BASE}/sync`, request)
  }

  static assignOptionTypes(request: { productId: string; items: OptionTypeSyncItem[] }): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.BASE}/assign`, request)
  }

  static revokeOptionTypes(request: { productId: string; items: OptionTypeSyncItem[] }): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.BASE}/revoke`, request)
  }
}
