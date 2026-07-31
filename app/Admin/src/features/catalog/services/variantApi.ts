import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  VariantRequest,
  VariantListItem,
  VariantDetail,
  VariantQuery,
  OptionValueAssignment,
} from '../types/variant'
import {
  toVariantQueryParams,
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantApi {
  static getVariants(
    productId: string,
    query: VariantQuery,
  ): Promise<PagedResult<VariantListItem>> {
    return getPaged<VariantListItem>(
      `${BASE}?productId=${productId}`,
      toVariantQueryParams(query),
      {
        allowedFilterFields: VARIANT_FILTER_FIELDS,
        allowedSortFields: VARIANT_SORT_FIELDS,
        allowedSearchFields: VARIANT_SEARCH_FIELDS,
      },
    )
  }

  static getVariant(id: string): Promise<Result<VariantDetail>> {
    return get<Result<VariantDetail>>(`${BASE}/${id}`)
  }

  static createVariant(
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return post<Result<VariantDetail>>(
      BASE,
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
  ): Promise<PagedResult<OptionValueAssignment>> {
    return getPaged<OptionValueAssignment>(
      `${BASE}/${variantId}/option-values`,
      { pageNumber: 1, pageSize: 100 },
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
