import { post, get, put, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type {
  VariantRequest,
  VariantDetail,
  OptionValueAssignment,
} from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantApi {
  static getVariants(
    productId: string,
  ): Promise<Result<{ items: VariantDetail[] }>> {
    return get<Result<{ items: VariantDetail[] }>>(
      `${CATALOG}/products/${productId}/variants`,
    )
  }

  static getVariant(id: string): Promise<Result<VariantDetail>> {
    return get<Result<VariantDetail>>(`${BASE}/${id}`)
  }

  static createVariant(
    productId: string,
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return post<Result<VariantDetail>>(
      `${CATALOG}/products/${productId}/variants`,
      request,
    )
  }

  static updateVariant(
    id: string,
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return put<Result<VariantDetail>>(`${BASE}/${id}`, request)
  }

  static deleteVariant(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${BASE}/${id}`)
  }

  static getOptionValues(
    variantId: string,
  ): Promise<Result<{ items: OptionValueAssignment[] }>> {
    return get<Result<{ items: OptionValueAssignment[] }>>(
      `${BASE}/${variantId}/option-values`,
    )
  }

  static assignOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      `${BASE}/${variantId}/option-values/assign`,
      { optionValueIds },
    )
  }

  static revokeOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      `${BASE}/${variantId}/option-values/revoke`,
      { optionValueIds },
    )
  }
}
