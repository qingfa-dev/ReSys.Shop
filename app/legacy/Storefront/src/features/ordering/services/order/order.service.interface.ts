import type { Result, PagedResult } from '@/core/models/result'
import type { Order, CheckoutRequest } from '../../types'

export interface IOrderService {
  getOrders(params?: { page?: number; pageSize?: number }): Promise<PagedResult<Order>>
  getOrder(id: string): Promise<Result<Order>>
  checkout(request: CheckoutRequest): Promise<Result<Order>>
}