import type { QueryingParameters } from '@/shared/types/querying'

export type AddressType = 'Shipping' | 'Billing' | 'Other'

export interface AddressRequest {
  userId: string
  addressType: AddressType
  firstName: string
  lastName?: string
  address1: string
  address2?: string
  city: string
  zipCode?: string
  phone?: string
  label?: string
  isDefault: boolean
  countryName: string
  stateProvince?: string
  countryCode?: string
  stateCode?: string
}

export interface AddressResponse extends AddressRequest {
  id: string
}

export interface AddressQuery {
  userId: string
  addressType?: AddressType
  isDefault?: boolean
  search?: string
  sortBy?: 'firstName' | 'city' | 'countryName' | 'addressType'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const ADDRESS_FILTER_FIELDS = [
  'addressType',
  'countryCode',
  'stateCode',
  'isDefault',
  'isDefaultBilling',
  'isDefaultShipping',
  'userProfileId',
]

export const ADDRESS_SORT_FIELDS = ['firstName', 'city', 'countryName', 'addressType']

export const ADDRESS_SEARCH_FIELDS = [
  'firstName',
  'lastName',
  'address1',
  'city',
  'countryName',
  'label',
  'phone',
]

export function toAddressQueryParams(query: AddressQuery): QueryingParameters {
  const filters: string[] = []

  if (query.addressType !== undefined) {
    filters.push(`addressType=${query.addressType}`)
  }
  if (query.isDefault !== undefined) {
    filters.push(`isDefault=${query.isDefault}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: ADDRESS_SEARCH_FIELDS,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
