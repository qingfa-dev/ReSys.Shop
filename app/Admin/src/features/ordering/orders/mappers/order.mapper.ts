import type { OrderListItem, OrderDetail } from '../types/order.response.type'
import { toOrderListItemModel, toOrderDetailModel, type OrderListItemModel, type OrderDetailModel } from '../types/order.model.type'

export function mapOrderListItem(dto: OrderListItem): OrderListItemModel {
  return toOrderListItemModel(dto)
}

export function mapOrderDetail(dto: OrderDetail): OrderDetailModel {
  return toOrderDetailModel(dto)
}
