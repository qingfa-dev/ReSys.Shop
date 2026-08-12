import { getPaged } from '@/shared/api'
import { get, post, put, del } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { AddressRequest, AddressResponse } from '../types/address'
import {
  ADDRESS_FILTER_FIELDS,
  ADDRESS_SORT_FIELDS,
  ADDRESS_SEARCH_FIELDS,
} from '../types/address'

export class AddressApi {
  static getAddresses(userId: string, params: QueryingParameters): Promise<PagedResult<AddressResponse>> {
    return getPaged<AddressResponse>(`/api/admin/customer/addresses?userId=${userId}`, params, {
      allowedFilterFields: ADDRESS_FILTER_FIELDS,
      allowedSortFields: ADDRESS_SORT_FIELDS,
      allowedSearchFields: ADDRESS_SEARCH_FIELDS,
    })
  }

  static getAddress(userId: string, id: string): Promise<Result<AddressResponse>> {
    return get<Result<AddressResponse>>(`/api/admin/customer/addresses/${id}?userId=${userId}`)
  }

  static createAddress(request: AddressRequest): Promise<Result<AddressResponse>> {
    return post<Result<AddressResponse>>('/api/admin/customer/addresses', request)
  }

  static updateAddress(id: string, request: AddressRequest): Promise<Result<AddressResponse>> {
    return put<Result<AddressResponse>>(`/api/admin/customer/addresses/${id}`, request)
  }

  static deleteAddress(userId: string, id: string): Promise<Result<{ id: string; label: string }>> {
    return del<Result<{ id: string; label: string }>>(`/api/admin/customer/addresses/${id}?userId=${userId}`)
  }
}