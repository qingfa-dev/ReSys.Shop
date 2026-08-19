import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { ShippingRateRequest, ShippingRateListItem, ShippingRateDetail } from '../types/shippingRate'
import { SHIPPING_RATE_FILTER_FIELDS, SHIPPING_RATE_SORT_FIELDS, SHIPPING_RATE_SEARCH_FIELDS } from '../types/shippingRate'

export class ShippingRateApi {
  static getShippingRates(params: QueryingParameters): Promise<PagedResult<ShippingRateListItem>> {
    return getPaged<ShippingRateListItem>('/api/admin/shipping/shipping-rates', params, {
      allowedFilterFields: SHIPPING_RATE_FILTER_FIELDS,
      allowedSortFields: SHIPPING_RATE_SORT_FIELDS,
      allowedSearchFields: SHIPPING_RATE_SEARCH_FIELDS,
    })
  }

  static getShippingRate(id: string): Promise<Result<ShippingRateDetail>> {
    return get<Result<ShippingRateDetail>>(`/api/admin/shipping/shipping-rates/${id}`)
  }

  static createShippingRate(request: ShippingRateRequest): Promise<Result<ShippingRateDetail>> {
    return post<Result<ShippingRateDetail>>('/api/admin/shipping/shipping-rates', request)
  }

  static updateShippingRate(id: string, request: ShippingRateRequest): Promise<Result<ShippingRateDetail>> {
    return put<Result<ShippingRateDetail>>(`/api/admin/shipping/shipping-rates/${id}`, request)
  }

  static deleteShippingRate(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/shipping/shipping-rates/${id}`)
  }
}