import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  VariantRequest,
  VariantListItem,
  VariantDetail,
  OptionValueAssignment,
} from '../types/variant'
import {
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../types/variant'

export class VariantApi {
  static getVariants(
    productId: string,
    params: QueryingParameters,
  ): Promise<PagedResult<VariantListItem>> {
    const url = productId ? `/api/admin/catalog/variants?productId=${productId}` : '/api/admin/catalog/variants'
    return getPaged<VariantListItem>(
      url,
      params,
      {
        allowedFilterFields: VARIANT_FILTER_FIELDS,
        allowedSortFields: VARIANT_SORT_FIELDS,
        allowedSearchFields: VARIANT_SEARCH_FIELDS,
      },
    )
  }

  static getVariant(id: string): Promise<Result<VariantDetail>> {
    return get<Result<VariantDetail>>(`/api/admin/catalog/variants/${id}`)
  }

  static createVariant(
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return post<Result<VariantDetail>>(
      '/api/admin/catalog/variants',
      request,
    )
  }

  static updateVariant(
    id: string,
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return put<Result<VariantDetail>>(`/api/admin/catalog/variants/${id}`, request)
  }

  static deleteVariant(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/catalog/variants/${id}`)
  }

  static getOptionValues(
    variantId: string,
  ): Promise<PagedResult<OptionValueAssignment>> {
    return getPaged<OptionValueAssignment>(
      `/api/admin/catalog/variant-option-values?variantId=${variantId}`,
      {},
    )
  }

  static assignOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      '/api/admin/catalog/variant-option-values/assign',
      { variantId, optionValueIds },
    )
  }

  static revokeOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      '/api/admin/catalog/variant-option-values/revoke',
      { variantId, optionValueIds },
    )
  }

  static syncOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return put<Result<void>>(
      '/api/admin/catalog/variant-option-values/sync',
      { variantId, optionValueIds },
    )
  }
}
