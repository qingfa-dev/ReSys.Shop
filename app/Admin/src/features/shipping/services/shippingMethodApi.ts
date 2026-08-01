import { post, get, put, del, patch } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { SHIPPING } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type { ShippingMethodRequest, ShippingMethodListItem, ShippingMethodDetail, ShippingMethodQuery } from '../types/shippingMethod'
import { toShippingMethodQueryParams, SHIPPING_METHOD_FILTER_FIELDS, SHIPPING_METHOD_SORT_FIELDS, SHIPPING_METHOD_SEARCH_FIELDS } from '../types/shippingMethod'

export class ShippingMethodApi {
  private static readonly BASE = `${SHIPPING}/shipping-methods`

  static getShippingMethods(query: ShippingMethodQuery): Promise<PagedResult<ShippingMethodListItem>> {
    return getPaged<ShippingMethodListItem>(ShippingMethodApi.BASE, toShippingMethodQueryParams(query), {
      allowedFilterFields: SHIPPING_METHOD_FILTER_FIELDS,
      allowedSortFields: SHIPPING_METHOD_SORT_FIELDS,
      allowedSearchFields: SHIPPING_METHOD_SEARCH_FIELDS,
    })
  }

  static getShippingMethod(id: string): Promise<Result<ShippingMethodDetail>> {
    return get<Result<ShippingMethodDetail>>(`${ShippingMethodApi.BASE}/${id}`)
  }

  static createShippingMethod(request: ShippingMethodRequest): Promise<Result<ShippingMethodDetail>> {
    return post<Result<ShippingMethodDetail>>(ShippingMethodApi.BASE, request)
  }

  static updateShippingMethod(id: string, request: ShippingMethodRequest): Promise<Result<ShippingMethodDetail>> {
    return put<Result<ShippingMethodDetail>>(`${ShippingMethodApi.BASE}/${id}`, request)
  }

  static deleteShippingMethod(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${ShippingMethodApi.BASE}/${id}`)
  }

  static activateShippingMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`${ShippingMethodApi.BASE}/${id}/activate`)
  }

  static deactivateShippingMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`${ShippingMethodApi.BASE}/${id}/deactivate`)
  }
}
