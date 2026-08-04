import { queryBuilder } from '@/core/helpers/query.builder'
import type { OrderEntity } from '../entity'

export interface OrderFilterParams {
  status?: string
  dateFrom?: string
  dateTo?: string
  search?: string
  page?: number
  pageSize?: number
}

export function buildOrderFilter(params: OrderFilterParams) {
  const builder = queryBuilder<OrderEntity>()

  if (params.status) {
    builder.where('status', '=', params.status)
  }
  if (params.dateFrom) {
    builder.where('createdAt', '>=', params.dateFrom)
  }
  if (params.dateTo) {
    builder.where('createdAt', '<=', params.dateTo)
  }
  if (params.search) {
    builder.search(params.search, ['orderNumber'])
  }

  builder.orderBy('createdAt', 'desc')

  if (params.page !== undefined && params.pageSize !== undefined) {
    builder.page(params.page, params.pageSize)
  }

  return builder.build()
}