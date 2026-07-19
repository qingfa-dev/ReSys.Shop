import type { OrderParameters } from '../schemas/order.schema'

export type CreateOrderRequest = OrderParameters

export interface AddOrderItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateAddressesRequest {
  shippingAddress?: Partial<OrderParameters['shippingAddress']>
  billingAddress?: Partial<OrderParameters['billingAddress']>
}

export interface CancelOrderRequest {
  reason?: string
}

export interface CreateShipmentRequest {
  stockLocationId: string
  inventoryUnitIds: string[]
}

export interface UpdateLineItemRequest {
  quantity?: number
}

export interface UpdateOrderStatusRequest {
  status: string
}

export interface UpdateShippingMethodRequest {
  shippingMethodId: string
}
