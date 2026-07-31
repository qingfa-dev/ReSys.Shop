import { post, get, put, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type {
  VariantRequest,
  Variant,
  OptionValueAssignment,
} from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantApi {
  static getVariants(
    productId: string,
  ): Promise<Result<{ items: Variant[] }>> {
    return get<Result<{ items: Variant[] }>>(
      `${CATALOG}/products/${productId}/variants`,
    )
  }

  static getVariant(id: string): Promise<Result<Variant>> {
    return get<Result<Variant>>(`${BASE}/${id}`)
  }

  static createVariant(
    productId: string,
    request: VariantRequest,
  ): Promise<Result<Variant>> {
    return post<Result<Variant>>(
      `${CATALOG}/products/${productId}/variants`,
      request,
    )
  }

  static updateVariant(
    id: string,
    request: VariantRequest,
  ): Promise<Result<Variant>> {
    return put<Result<Variant>>(`${BASE}/${id}`, request)
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
