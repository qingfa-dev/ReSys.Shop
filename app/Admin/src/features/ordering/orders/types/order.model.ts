import type { OrderListItem, OrderDetail } from './order.response'
import { OrderStatusMap, PaymentStateMap, ShipmentStateMap } from '@/shared/utils/enums'
import { decimalToDisplay } from '@/shared/utils/currency'

export interface OrderListItemModel extends OrderListItem {
  totalDisplay: string
  statusLabel: string
  paymentStateLabel: string | null
  shipmentStateLabel: string | null
}

export interface OrderDetailModel extends OrderDetail {
  totalDisplay: string
  itemTotalDisplay: string
  shipmentTotalDisplay: string
  adjustmentTotalDisplay: string
  outstandingBalanceDisplay: string
  statusLabel: string
  paymentStateLabel: string | null
  shipmentStateLabel: string | null
}

export function toOrderListItemModel(dto: OrderListItem): OrderListItemModel {
  return {
    ...dto,
    totalDisplay: decimalToDisplay(dto.total),
    statusLabel: OrderStatusMap[dto.status] ?? 'Unknown',
    paymentStateLabel: dto.paymentState != null ? PaymentStateMap[dto.paymentState] ?? 'Unknown' : null,
    shipmentStateLabel: dto.shipmentState != null ? ShipmentStateMap[dto.shipmentState] ?? 'Unknown' : null,
  }
}

export function toOrderDetailModel(dto: OrderDetail): OrderDetailModel {
  return {
    ...dto,
    totalDisplay: decimalToDisplay(dto.total),
    itemTotalDisplay: decimalToDisplay(dto.itemTotal),
    shipmentTotalDisplay: decimalToDisplay(dto.shipmentTotal),
    adjustmentTotalDisplay: decimalToDisplay(dto.adjustmentTotal),
    outstandingBalanceDisplay: decimalToDisplay(dto.outstandingBalance),
    statusLabel: OrderStatusMap[dto.status] ?? 'Unknown',
    paymentStateLabel: dto.paymentState != null ? PaymentStateMap[dto.paymentState] ?? 'Unknown' : null,
    shipmentStateLabel: dto.shipmentState != null ? ShipmentStateMap[dto.shipmentState] ?? 'Unknown' : null,
  }
}
