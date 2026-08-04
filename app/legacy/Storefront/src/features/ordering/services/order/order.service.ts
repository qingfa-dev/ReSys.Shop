import { orderApiRepository } from '../../repositories/order/order.api'
import type { IOrderService } from './order.service.interface'
import type { Order, CheckoutRequest } from '../../types'
import type { Result, PagedResult } from '@/core/models/result'
import { mapOrderResponseToEntity } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

export class OrderService implements IOrderService {
  private orderRepo = orderApiRepository

  async getOrders(params?: { page?: number; pageSize?: number }): Promise<PagedResult<Order>> {
    const response = await this.orderRepo.getAll({ paging: params ? { page: params.page ?? 1, pageSize: params.pageSize ?? 10 } : undefined })
    return {
      ...response,
      items: response.items.map(mapOrderResponseToEntity),
    }
  }

  async getOrder(id: string): Promise<Result<Order>> {
    const response = await this.orderRepo.getById(id)
    return resultMap(response, mapOrderResponseToEntity)
  }

  async checkout(request: CheckoutRequest): Promise<Result<Order>> {
    const response = await this.orderRepo.checkout(request)
    return resultMap(response, mapOrderResponseToEntity)
  }
}

export const orderService = new OrderService()