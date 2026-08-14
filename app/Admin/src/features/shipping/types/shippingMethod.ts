import type { QueryingParameters } from '@/shared/types/querying'

export interface ShippingMethodRequest {
  name: string
  code?: string
  trackingUrl?: string
  adminName?: string
  position: number
  availableToUsers: boolean
  calculatorType: string
  presentation?: string
}

export interface ShippingMethodListItem extends ShippingMethodRequest {
  id: string
  createdAtUtc: string
  modifiedAtUtc?: string
  createdBy?: string
  modifiedBy?: string
  isDeleted: boolean
  deletedAtUtc?: string
}

export type ShippingMethodDetail = ShippingMethodListItem

export interface ShippingMethodQuery {
  availableToUsers?: boolean
  calculatorType?: string
  search?: string
  sortBy?: 'name' | 'code' | 'position' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const SHIPPING_METHOD_FILTER_FIELDS = [
  'availableToUsers',
  'calculatorType',
  'isDeleted',
]

export const SHIPPING_METHOD_SORT_FIELDS = [
  'name',
  'code',
  'position',
  'createdAtUtc',
]

export const SHIPPING_METHOD_SEARCH_FIELDS = ['name', 'code', 'adminName']

export function toShippingMethodQueryParams(query: ShippingMethodQuery): QueryingParameters {
  const filters: string[] = []

  if (query.availableToUsers !== undefined) {
    filters.push(`availableToUsers=${query.availableToUsers}`)
  }
  if (query.calculatorType !== undefined && query.calculatorType !== '') {
    filters.push(`calculatorType=${query.calculatorType}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
