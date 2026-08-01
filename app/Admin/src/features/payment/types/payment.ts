import type { QueryingParameters } from '@/shared/types/querying'

export interface PaymentListItem {
  id: string
  amount: number
  currency: string
  orderId: string
  paymentMethodId: string
  state: string
  paymentStatus?: string
}

export interface PaymentDetail extends PaymentListItem {
  number: string
  responseCode?: string
  paymentMethodName?: string
  clientSecret?: string
  createdAtUtc: string
  modifiedAtUtc?: string
  createdBy?: string
  modifiedBy?: string
}

export interface PaymentQuery {
  state?: string
  paymentMethodId?: string
  orderId?: string
  search?: string
  sortBy?: 'number' | 'amount' | 'state' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const PAYMENT_FILTER_FIELDS = ['state', 'paymentMethodId', 'orderId']

export const PAYMENT_SORT_FIELDS = ['number', 'amount', 'state', 'createdAtUtc']

export const PAYMENT_SEARCH_FIELDS = ['number']

export function toPaymentQueryParams(query: PaymentQuery): QueryingParameters {
  const filters: string[] = []

  if (query.state !== undefined && query.state !== '') {
    filters.push(`state=${query.state}`)
  }
  if (query.paymentMethodId !== undefined && query.paymentMethodId !== '') {
    filters.push(`paymentMethodId=${query.paymentMethodId}`)
  }
  if (query.orderId !== undefined && query.orderId !== '') {
    filters.push(`orderId=${query.orderId}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: PAYMENT_SEARCH_FIELDS,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}

export interface CapturePaymentRequest {
  amount?: number
}

export interface CapturePaymentResponse extends PaymentDetail {
  capturedAmount: number
  message: string
}

export interface RefundPaymentRequest {
  amount: number
  reason?: string
}

export interface RefundPaymentResponse extends PaymentDetail {
  refundedAmount: number
  message: string
}

export interface VoidPaymentResponse extends PaymentDetail {
  message: string
}
