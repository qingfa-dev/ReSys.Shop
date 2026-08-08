import { get, put } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { ORDERS } from '@/shared/constants/api'
import { OrderListItemSchema, OrderDetailSchema, OrderTrackingResponseSchema } from '../validations/order'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult, Result } from '@/shared/types'
import type { OrderListItem, OrderDetail, OrderTrackingResponse } from '../types'

const orderListSchema = PagedResultSchema(OrderListItemSchema)

export class OrderApi {
  private static readonly BASE = ORDERS

  static async getOrders(params: Record<string, unknown>): Promise<PagedResult<OrderListItem>> {
    const result = await getPaged<unknown>(this.BASE, params)
    if (!result.isSuccess) return result as PagedResult<OrderListItem>
    const parsed = orderListSchema.parse({ ...result, items: result.items })
    return parsed as PagedResult<OrderListItem>
  }

  static async getOrder(id: string): Promise<Result<OrderDetail>> {
    const result = await get<Result<OrderDetail>>(`${this.BASE}/${id}`)
    if (!result.isSuccess) return result
    result.value = OrderDetailSchema.parse(result.value)
    return result
  }

  static async getOrderTracking(id: string): Promise<Result<OrderTrackingResponse>> {
    const result = await get<Result<OrderTrackingResponse>>(`${this.BASE}/${id}/tracking`)
    if (!result.isSuccess) return result
    result.value = OrderTrackingResponseSchema.parse(result.value)
    return result
  }

  static async cancelOrder(id: string): Promise<Result<null>> {
    return await put<Result<null>>(`${this.BASE}/${id}/cancel`)
  }
}
