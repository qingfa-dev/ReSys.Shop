import type { QueryingParameters } from '@/shared/types/querying'

export type StockTransferState = 'Draft' | 'InTransit' | 'Received' | 'Canceled'

export interface StockTransferItemRequest {
  variantId: string
  quantity: number
}

export interface StockTransferRequest {
  reference?: string
  sourceLocationId: string
  destinationLocationId: string
  items: StockTransferItemRequest[]
}

export interface StockTransferListItem {
  id: string
  number: string
  reference?: string
  sourceLocationId: string
  destinationLocationId: string
  state: StockTransferState
  totalItems: number
  createdAtUtc: string
}

export interface StockTransferDetail {
  id: string
  number: string
  reference?: string
  sourceLocationId: string
  destinationLocationId: string
  state: StockTransferState
  createdAtUtc: string
  modifiedAtUtc?: string
  items: {
    variantId: string
    quantity: number
    receivedQuantity: number
  }[]
}

export interface StockTransferReceiveRequest {
  items: {
    variantId: string
    quantity: number
  }[]
}

export interface StockTransferQuery {
  state?: StockTransferState
  sourceLocationId?: string
  destinationLocationId?: string
  sortBy?: 'number' | 'state' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STOCK_TRANSFER_FILTER_FIELDS = ['State', 'SourceLocationId', 'DestinationLocationId']

export const STOCK_TRANSFER_SORT_FIELDS = ['Number', 'State', 'CreatedAtUtc']

export const STOCK_TRANSFER_SEARCH_FIELDS: string[] = []

export function toStockTransferQueryParams(query: StockTransferQuery): QueryingParameters {
  const filters: string[] = []

  if (query.state !== undefined) {
    filters.push(`State=${query.state}`)
  }
  if (query.sourceLocationId !== undefined && query.sourceLocationId !== '') {
    filters.push(`SourceLocationId=${query.sourceLocationId}`)
  }
  if (query.destinationLocationId !== undefined && query.destinationLocationId !== '') {
    filters.push(`DestinationLocationId=${query.destinationLocationId}`)
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
