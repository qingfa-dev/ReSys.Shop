import { post, get, put, del, patch } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { ShippingMethodRequest, ShippingMethodListItem, ShippingMethodDetail } from '../types/shippingMethod'
import { SHIPPING_METHOD_FILTER_FIELDS, SHIPPING_METHOD_SORT_FIELDS, SHIPPING_METHOD_SEARCH_FIELDS } from '../types/shippingMethod'

export class ShippingMethodApi {
  static getShippingMethods(params: QueryingParameters): Promise<PagedResult<ShippingMethodListItem>> {
    return getPaged<ShippingMethodListItem>('/api/admin/shipping/shipping-methods', params, {
      allowedFilterFields: SHIPPING_METHOD_FILTER_FIELDS,
      allowedSortFields: SHIPPING_METHOD_SORT_FIELDS,
      allowedSearchFields: SHIPPING_METHOD_SEARCH_FIELDS,
    })
  }

  static getShippingMethod(id: string): Promise<Result<ShippingMethodDetail>> {
    return get<Result<ShippingMethodDetail>>(`/api/admin/shipping/shipping-methods/${id}`)
  }

  static createShippingMethod(request: ShippingMethodRequest): Promise<Result<ShippingMethodDetail>> {
    return post<Result<ShippingMethodDetail>>('/api/admin/shipping/shipping-methods', request)
  }

  static updateShippingMethod(id: string, request: ShippingMethodRequest): Promise<Result<ShippingMethodDetail>> {
    return put<Result<ShippingMethodDetail>>(`/api/admin/shipping/shipping-methods/${id}`, request)
  }

  static deleteShippingMethod(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/shipping/shipping-methods/${id}`)
  }

  static activateShippingMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/shipping/shipping-methods/${id}/activate`)
  }

  static deactivateShippingMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/shipping/shipping-methods/${id}/deactivate`)
  }
}