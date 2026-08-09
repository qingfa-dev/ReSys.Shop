import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type { ShippingRateRequest, ShippingRateListItem, ShippingRateDetail, ShippingRateQuery } from '../types/shippingRate'
import { toShippingRateQueryParams, SHIPPING_RATE_FILTER_FIELDS, SHIPPING_RATE_SORT_FIELDS, SHIPPING_RATE_SEARCH_FIELDS } from '../types/shippingRate'

export class ShippingRateApi {
  private static readonly BASE = 'api/admin/shipping/shipping-rates'

  static getShippingRates(query: ShippingRateQuery): Promise<PagedResult<ShippingRateListItem>> {
    return getPaged<ShippingRateListItem>(ShippingRateApi.BASE, toShippingRateQueryParams(query), {
      allowedFilterFields: SHIPPING_RATE_FILTER_FIELDS,
      allowedSortFields: SHIPPING_RATE_SORT_FIELDS,
      allowedSearchFields: SHIPPING_RATE_SEARCH_FIELDS,
    })
  }

  static getShippingRate(id: string): Promise<Result<ShippingRateDetail>> {
    return get<Result<ShippingRateDetail>>(`${ShippingRateApi.BASE}/${id}`)
  }

  static createShippingRate(request: ShippingRateRequest): Promise<Result<ShippingRateDetail>> {
    return post<Result<ShippingRateDetail>>(ShippingRateApi.BASE, request)
  }

  static updateShippingRate(id: string, request: ShippingRateRequest): Promise<Result<ShippingRateDetail>> {
    return put<Result<ShippingRateDetail>>(`${ShippingRateApi.BASE}/${id}`, request)
  }

  static deleteShippingRate(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${ShippingRateApi.BASE}/${id}`)
  }
}
