import { getPaged } from '@/shared/api'
import { get, post, put, del } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  OrderRequest,
  OrderListItem,
  OrderDetail,
  LineItem,
  AddLineItemRequest,
  UpdateLineItemRequest,
  CancelOrderRequest,
  UpdateOrderAddressRequest,
  UpdateOrderShippingMethodRequest,
  UpdateOrderStatusRequest,
  Shipment,
  ShipmentStatus,
} from '../types/order'
import {
  ORDER_FILTER_FIELDS,
  ORDER_SORT_FIELDS,
  ORDER_SEARCH_FIELDS,
} from '../types/order'

export class OrderApi {
  static getOrders(params: QueryingParameters): Promise<PagedResult<OrderListItem>> {
    return getPaged<OrderListItem>('/api/admin/ordering/orders', params, {
      allowedFilterFields: ORDER_FILTER_FIELDS,
      allowedSortFields: ORDER_SORT_FIELDS,
      allowedSearchFields: ORDER_SEARCH_FIELDS,
    })
  }

  static getOrder(id: string): Promise<Result<OrderDetail>> {
    return get<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}`)
  }

  static createOrder(request: OrderRequest ): Promise<Result<OrderDetail>> {
    return post<Result<OrderDetail>>('/api/admin/ordering/orders', request)
  }

  static updateOrder(id: string, request: OrderRequest): Promise<Result<OrderDetail>> {
    return put<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}`, request)
  }

  static deleteOrder(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/ordering/orders/${id}`)
  }

  static getLineItems(id: string, params: QueryingParameters): Promise<PagedResult<LineItem>> {
    return getPaged<LineItem>(`/api/admin/ordering/orders/${id}/line-items`, params, {
      allowedFilterFields: ['OrderId', 'VariantId'],
      allowedSortFields: ['Quantity', 'Price', 'Total', 'CreatedAtUtc'],
      allowedSearchFields: [],
    })
  }

  static getLineItem(id: string, lineItemId: string): Promise<Result<LineItem>> {
    return get<Result<LineItem>>(`/api/admin/ordering/orders/${id}/line-items/${lineItemId}`)
  }

  static addLineItem(id: string, request: AddLineItemRequest): Promise<Result<OrderDetail>> {
    return post<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/line-items`, request)
  }

  static updateLineItem(id: string, lineItemId: string, request: UpdateLineItemRequest): Promise<Result<OrderDetail>> {
    return put<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/line-items/${lineItemId}`, request)
  }

  static removeLineItem(id: string, lineItemId: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/ordering/orders/${id}/line-items/${lineItemId}`)
  }

  static cancelOrder(id: string, request?: CancelOrderRequest): Promise<Result<OrderDetail>> {
    return post<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/cancel`, request ?? {})
  }

  static completeOrder(id: string): Promise<Result<OrderDetail>> {
    return post<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/complete`)
  }

  static approveOrder(id: string): Promise<Result<OrderDetail>> {
    return post<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/approve`)
  }

  static resumeOrder(id: string): Promise<Result<OrderDetail>> {
    return post<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/resume`)
  }

  static updateShipAddress(id: string, request: UpdateOrderAddressRequest): Promise<Result<OrderDetail>> {
    return put<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/ship-address`, request)
  }

  static updateBillAddress(id: string, request: UpdateOrderAddressRequest): Promise<Result<OrderDetail>> {
    return put<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/bill-address`, request)
  }

  static updateShippingMethod(id: string, request: UpdateOrderShippingMethodRequest): Promise<Result<OrderDetail>> {
    return put<Result<OrderDetail>>(`/api/admin/ordering/orders/${id}/shipping-method`, request)
  }

  static updateStatus(id: string, request: UpdateOrderStatusRequest): Promise<Result<void>> {
    return put<Result<void>>(`/api/admin/ordering/orders/${id}/status`, request)
  }

  static listShipments(orderId: string): Promise<Result<{ items: Shipment[] }>> {
    return get<Result<{ items: Shipment[] }>>(`/api/admin/shipping/shipments?orderId=${orderId}`)
  }

  static updateShipmentStatus(id: string, body: { status: ShipmentStatus; trackingNumber?: string }): Promise<Result<Shipment>> {
    return put<Result<Shipment>>(`/api/admin/shipping/shipments/${id}/status`, body)
  }
}
