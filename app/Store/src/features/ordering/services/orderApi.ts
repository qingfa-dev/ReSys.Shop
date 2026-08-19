import { get, post } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { OrderListItemSchema, OrderDetailSchema, OrderTrackingResponseSchema } from '../validations/order'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult, Result } from '@/shared/types'
import type { OrderListItem, OrderDetail, OrderTrackingResponse } from '../types'

const orderListSchema = PagedResultSchema(OrderListItemSchema)

// Service: Order API client for list, detail, tracking, and cancellation.
export class OrderApi {
  // Call: Fetch paginated order list — validates both items and paged envelope.
  static async getOrders(params: Record<string, unknown>): Promise<PagedResult<OrderListItem>> {
    const result = await getPaged<unknown>('/api/storefront/orders', params)
    if (!result.isSuccess) return result as PagedResult<OrderListItem>
    // Validate: Parse paged result with item-level schema for full type safety.
    const parsed = orderListSchema.parse({ ...result, items: result.items })
    return parsed as PagedResult<OrderListItem>
  }

  static async getOrder(id: string): Promise<Result<OrderDetail>> {
    const result = await get<Result<OrderDetail>>(`/api/storefront/orders/${id}`)
    if (!result.isSuccess) return result
    result.value = OrderDetailSchema.parse(result.value) as OrderDetail
    return result
  }

  static async getOrderTracking(id: string): Promise<Result<OrderTrackingResponse>> {
    const result = await get<Result<OrderTrackingResponse>>(`/api/storefront/orders/${id}/tracking`)
    if (!result.isSuccess) return result
    result.value = OrderTrackingResponseSchema.parse(result.value)
    return result
  }

  static async cancelOrder(id: string): Promise<Result<null>> {
    return await post<Result<null>>(`/api/storefront/orders/${id}/cancel`)
  }
}
