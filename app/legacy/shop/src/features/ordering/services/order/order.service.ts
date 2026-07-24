import { orderApiRepository } from '../../repositories/order/order.api'
import { mockOrderRepository } from '../../repositories/order/order.mock.repository'
import type { IOrderService } from './order.service.interface'
import type { Order, CheckoutRequest } from '../../types'
import type { Result, PagedResult } from '@/core/models/result'
import { mapOrderResponseToEntity } from '../../mapping'
import { resultMap, succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class OrderService implements IOrderService {
  private orderRepo = USE_MOCK ? mockOrderRepository : orderApiRepository

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

  async cancelOrder(id: string): Promise<Result<Order>> {
    const response = await this.orderRepo.cancelOrder(id)
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to cancel order', response.statusCode, response.errors)
    }
    return succeed(mapOrderResponseToEntity(response.data!), response.statusCode)
  }
}

export const orderService = new OrderService()