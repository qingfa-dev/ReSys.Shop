import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { AddressDetail } from './order.domain.types'

export interface OrderSearchParams extends ServerQueryingParameters {
  state?: string
  storeId?: string
  warehouseId?: string
  fromDate?: string
  toDate?: string
}

export interface CreateOrderRequest {
  email: string;
  currency?: string;
  lineItems: Array<{ variantId: string; quantity: number }>;
}

export interface AddOrderItemRequest {
  variantId: string;
  quantity: number;
}

export interface UpdateAddressesRequest {
  shippingAddress?: Partial<AddressDetail>;
  billingAddress?: Partial<AddressDetail>;
}

export interface CancelOrderRequest {
  reason?: string;
}

export interface CreateShipmentRequest {
  stockLocationId: string;
  inventoryUnitIds: string[];
}

export interface RefundPaymentRequest {
  amountCents: number;
  reason: string;
}
