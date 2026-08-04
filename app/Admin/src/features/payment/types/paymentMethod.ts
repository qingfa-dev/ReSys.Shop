import type { QueryingParameters } from '@/shared/types/querying'

export type DisplayOn = 'Both' | 'Frontend' | 'Backend'

export interface PaymentMethodRequest {
  name: string
  code?: string
  description?: string
  providerKey: string
  settings?: Record<string, string>
  preferences?: Record<string, string>
  webhookEnabled: boolean
  autoCapture: boolean
  displayOn: DisplayOn
  position: number
  presentation?: string
  active: boolean
}

export interface PaymentMethodUpdateRequest {
  name?: string
  code?: string
  description?: string
  providerKey?: string
  autoCapture?: boolean
  displayOn?: DisplayOn
  position?: number
  presentation?: string
  active?: boolean
  settings?: Record<string, string>
  preferences?: Record<string, string>
  webhookEnabled?: boolean
}

export interface PaymentMethodListItem extends PaymentMethodRequest {
  id: string
  createdAtUtc: string
  modifiedAtUtc?: string
}

export interface PaymentMethodDetail extends PaymentMethodListItem {
  createdBy?: string
  modifiedBy?: string
}

export interface PaymentMethodQuery {
  active?: boolean
  providerKey?: string
  autoCapture?: boolean
  search?: string
  sortBy?: 'name' | 'position' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const PAYMENT_METHOD_FILTER_FIELDS = [
  'active',
  'providerKey',
  'autoCapture',
  'displayOn',
  'isDeleted',
]

export const PAYMENT_METHOD_SORT_FIELDS = ['name', 'position', 'createdAtUtc']

export const PAYMENT_METHOD_SEARCH_FIELDS = ['name', 'code', 'description']

export function toPaymentMethodQueryParams(query: PaymentMethodQuery): QueryingParameters {
  const filters: string[] = []

  if (query.active !== undefined) {
    filters.push(`active=${query.active}`)
  }
  if (query.providerKey !== undefined && query.providerKey !== '') {
    filters.push(`providerKey=${query.providerKey}`)
  }
  if (query.autoCapture !== undefined) {
    filters.push(`autoCapture=${query.autoCapture}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: PAYMENT_METHOD_SEARCH_FIELDS,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
