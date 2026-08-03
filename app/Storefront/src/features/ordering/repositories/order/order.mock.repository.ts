import type { OrderResponse, OrderSingleResponse, OrderListResponse, AddressResponse } from '../../types/response'
import type { CheckoutRequest } from '../../types/request'
import { mockOrders, getOrderById } from '../../data/mock-orders.data'
import { filterByOperator, searchByFields, sortByField, paginateResults, buildFilters, createSearchConfig, createSortConfig } from '@/core/helpers/mock-query.helper'
import type { PagingParams, FilterParams, SearchParams, SortParams } from '@/core/models'

export interface OrderQueryParams {
  paging?: PagingParams
  filter?: FilterParams
  search?: SearchParams
  sort?: SortParams
}

function mapToOrderResponse(order: typeof mockOrders[0]): OrderResponse {
  return {
    id: order.id,
    orderNumber: order.orderNumber,
    status: order.status,
    items: order.items.map((item) => ({
      id: item.id,
      productId: item.productId,
      productName: item.productName,
      productImage: item.productImage,
      variantName: item.variantName,
      quantity: item.quantity,
      price: item.price,
    })),
    shippingAddress: order.shippingAddress,
    billingAddress: order.billingAddress,
    subtotal: order.subtotal,
    tax: order.tax,
    shipping: order.shipping,
    discount: order.discount,
    total: order.total,
    currency: order.currency,
    createdAt: order.createdAt,
    updatedAt: order.updatedAt,
  }
}

export class MockOrderRepository {
  async getAll(params?: OrderQueryParams): Promise<OrderListResponse> {
    const page = params?.paging?.page ?? 1
    const pageSize = params?.paging?.pageSize ?? 10
    let result = mockOrders.map(mapToOrderResponse)

    if (params?.filter?.filter) {
      const parsedFilter = JSON.parse(params.filter.filter)
      const filters = buildFilters<OrderResponse>(parsedFilter)
      result = filterByOperator(result, filters)
    }
    if (params?.search?.search && params.search.searchFields?.length) {
      const searchConfig = createSearchConfig<OrderResponse>(params.search.search, params.search.searchFields)
      result = searchByFields(result, searchConfig)
    }
    if (params?.sort?.sortBy) {
      const sortConfig = createSortConfig<OrderResponse>(params.sort.sortBy, params.sort.sortOrder ?? 'asc')
      result = sortByField(result, sortConfig)
    }

    const { items, meta } = paginateResults(result, page, pageSize)
    return { isSuccess: true, isFailure: false, statusCode: 200, items, page: meta.page, pageSize: meta.pageSize, totalCount: meta.totalCount, totalPages: meta.totalPages, hasNextPage: meta.hasNextPage, hasPreviousPage: meta.hasPreviousPage }
  }

  async getById(id: string): Promise<OrderSingleResponse> {
    const order = getOrderById(id)
    if (!order) return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Order not found' }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToOrderResponse(order) }
  }

  async checkout(_request: CheckoutRequest): Promise<OrderSingleResponse> {
    const shippingAddress: AddressResponse = { id: '', firstName: '', address1: '', city: '' }
    const billingAddress: AddressResponse = { id: '', firstName: '', address1: '', city: '' }
    return { isSuccess: true, isFailure: false, statusCode: 201, data: { id: `order-${Date.now()}`, orderNumber: `ORD-${Date.now()}`, status: 'pending', items: [], shippingAddress, billingAddress, subtotal: 0, tax: 0, shipping: 0, discount: 0, total: 0, currency: 'USD', createdAt: new Date().toISOString(), updatedAt: '' } }
  }

  async cancelOrder(id: string): Promise<OrderSingleResponse> {
    const order = getOrderById(id)
    if (!order) return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Order not found' }
    const cancelledOrder = { ...order, status: 'cancelled' as const, updatedAt: new Date().toISOString() }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToOrderResponse(cancelledOrder) }
  }
}

export const mockOrderRepository = new MockOrderRepository()