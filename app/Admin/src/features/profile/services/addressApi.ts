import { getPaged } from '@/shared/api'
import { get, post, put, del } from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/types'
import type { AddressQuery, AddressRequest, AddressResponse } from '../types/address'
import {
  toAddressQueryParams,
  ADDRESS_FILTER_FIELDS,
  ADDRESS_SORT_FIELDS,
  ADDRESS_SEARCH_FIELDS,
} from '../types/address'

export class AddressApi {
  private static readonly BASE = 'api/admin/customer/addresses'

  static getAddresses(userId: string, query: AddressQuery): Promise<PagedResult<AddressResponse>> {
    return getPaged<AddressResponse>(`${AddressApi.BASE}?userId=${userId}`, toAddressQueryParams(query), {
      allowedFilterFields: ADDRESS_FILTER_FIELDS,
      allowedSortFields: ADDRESS_SORT_FIELDS,
      allowedSearchFields: ADDRESS_SEARCH_FIELDS,
    })
  }

  static getAddress(userId: string, id: string): Promise<Result<AddressResponse>> {
    return get<Result<AddressResponse>>(`${AddressApi.BASE}/${id}?userId=${userId}`)
  }

  static createAddress(request: AddressRequest): Promise<Result<AddressResponse>> {
    return post<Result<AddressResponse>>(AddressApi.BASE, request)
  }

  static updateAddress(id: string, request: AddressRequest): Promise<Result<AddressResponse>> {
    return put<Result<AddressResponse>>(`${AddressApi.BASE}/${id}`, request)
  }

  static deleteAddress(userId: string, id: string): Promise<Result<{ id: string; label: string }>> {
    return del<Result<{ id: string; label: string }>>(`${AddressApi.BASE}/${id}?userId=${userId}`)
  }
}
