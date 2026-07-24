import { BaseRepository } from '@/core/repositories'
import type { Result, PagedResult } from '@/core/models/result'
import type { OrderResponse } from '../../types/response'
import type { CheckoutRequest } from '../../types/request'
import type { IOrderRepository } from './order.repository.interface'

export class OrderApiRepository extends BaseRepository implements IOrderRepository {
  protected readonly endpoint = '/ordering/orders'

  async getAll(params?: { paging?: { page: number; pageSize: number }; filter?: { filter: string }; search?: { search: string; searchFields: string[] }; sort?: { sortBy: string; sortOrder: 'asc' | 'desc' } }): Promise<PagedResult<OrderResponse>> {
    return super.getPaged<OrderResponse>(this.endpoint, params?.paging, params?.filter, params?.search, params?.sort)
  }

  getById<T = OrderResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>(this.endpoint, id)
  }

  async checkout(request: CheckoutRequest): Promise<Result<OrderResponse>> {
    return this.post<OrderResponse>(`${this.endpoint}/checkout`, request)
  }

  async cancelOrder(id: string): Promise<Result<OrderResponse>> {
    return this.patch<OrderResponse>(`${this.endpoint}/${id}`, { status: 'cancelled' })
  }
}

export const orderApiRepository = new OrderApiRepository()