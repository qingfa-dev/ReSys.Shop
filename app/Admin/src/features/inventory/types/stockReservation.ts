import type { QueryingParameters } from '@/shared/types/querying'

export type ReservationState = 'Reserved' | 'Fulfilled' | 'Released' | 'Expired'

export interface StockReservationListItem {
  id: string
  variantId: string
  stockLocationId?: string
  orderId?: string
  quantity: number
  state: ReservationState
  expiresAtUtc?: string
  reason?: string
  createdAtUtc: string
}

export interface StockReservationDetail extends StockReservationListItem {
  modifiedAtUtc?: string
}

export interface StockReservationQuery {
  variantId?: string
  orderId?: string
  state?: ReservationState
  sortBy?: 'expiresAtUtc' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STOCK_RESERVATION_FILTER_FIELDS = ['variantId', 'orderId', 'state']

export const STOCK_RESERVATION_SORT_FIELDS = ['expiresAtUtc', 'createdAtUtc']

export const STOCK_RESERVATION_SEARCH_FIELDS: string[] = []

export function toStockReservationQueryParams(query: StockReservationQuery): QueryingParameters {
  const filters: string[] = []

  if (query.variantId !== undefined && query.variantId !== '') {
    filters.push(`variantId=${query.variantId}`)
  }
  if (query.orderId !== undefined && query.orderId !== '') {
    filters.push(`orderId=${query.orderId}`)
  }
  if (query.state !== undefined) {
    filters.push(`state=${query.state}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
