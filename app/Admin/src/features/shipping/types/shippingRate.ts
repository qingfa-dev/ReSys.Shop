import type { QueryingParameters } from '@/shared/types/querying'

export interface ShippingRateRequest {
  name: string
  presentation?: string
  cost: number
  deliveryRange?: string
  minWeight?: number
  maxWeight?: number
  freeShippingThreshold?: number
  shippingMethodId: string
}

export interface ShippingRateListItem extends ShippingRateRequest {
  id: string
  finalPrice: number
  selected: boolean
  createdAtUtc: string
  modifiedAtUtc?: string
  createdBy?: string
  modifiedBy?: string
}

export type ShippingRateDetail = ShippingRateListItem

export interface ShippingRateQuery {
  shippingMethodId?: string
  selected?: boolean
  search?: string
  sortBy?: 'name' | 'cost' | 'finalPrice' | 'selected' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const SHIPPING_RATE_FILTER_FIELDS = ['selected', 'shippingMethodId']

export const SHIPPING_RATE_SORT_FIELDS = ['name', 'cost', 'finalPrice', 'selected', 'createdAtUtc']

export const SHIPPING_RATE_SEARCH_FIELDS = ['name', 'deliveryRange']

export function toShippingRateQueryParams(query: ShippingRateQuery): QueryingParameters {
  const filters: string[] = []

  if (query.shippingMethodId !== undefined && query.shippingMethodId !== '') {
    filters.push(`shippingMethodId=${query.shippingMethodId}`)
  }
  if (query.selected !== undefined) {
    filters.push(`selected=${query.selected}`)
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
