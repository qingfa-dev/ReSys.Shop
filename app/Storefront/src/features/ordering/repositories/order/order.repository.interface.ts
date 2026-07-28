import type { Result, PagedResult } from '@/core/models/result'
import type { OrderResponse } from '../../types/response'
import type { CheckoutRequest } from '../../types/request'

export interface OrderQueryParams {
  paging?: { page: number; pageSize: number }
  filter?: { filter: string }
  search?: { search: string; searchFields: string[] }
  sort?: { sortBy: string; sortOrder: 'asc' | 'desc' }
}

export interface IOrderRepository {
  getAll(params?: OrderQueryParams): Promise<PagedResult<OrderResponse>>
  getById<T = OrderResponse>(id: string): Promise<Result<T>>
  checkout(request: CheckoutRequest): Promise<Result<OrderResponse>>
}