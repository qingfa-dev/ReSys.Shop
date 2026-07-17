export interface OrderListItem {
  id: string;
  number: string;
  state: string;
  currency: string;
  totalCents: number;
  totalDisplay: string;
  email?: string;
  paymentState?: string;
  shipmentState?: string;
  createdAtUtc: string;
}

export interface AddressDetail {
  id: string;
  firstName: string;
  lastName: string;
  address1: string;
  address2?: string;
  city: string;
  zipCode: string;
  countryCode: string;
  stateCode?: string;
  phone?: string;
  company?: string;
}

export interface LineItemDetail {
  id: string;
  variantId: string;
  name: string;
  sku: string;
  quantity: number;
  unitPriceCents: number;
  unitPriceDisplay: string;
  totalCents: number;
  totalDisplay: string;
  inventoryUnits: InventoryUnitDetail[];
}

export interface PaymentDetail {
  id: string;
  amountCents: number;
  amountDisplay: string;
  state: string;
  methodType: string;
  transactionId?: string;
  createdAtUtc: string;
}

export interface InventoryUnitDetail {
  id: string;
  sku: string;
  state: string;
  serialNumber?: string;
  pending: boolean;
}

export interface ShipmentDetail {
  id: string;
  number: string;
  state: string;
  trackingNumber?: string;
  stockLocationId: string;
  stockLocationName?: string;
  units: InventoryUnitDetail[];
}

export interface OrderHistoryDetail {
  description: string;
  fromState?: string;
  toState: string;
  triggeredBy?: string;
  createdAtUtc: string;
  context: Record<string, any>;
}

export interface OrderDetail extends OrderListItem {
  itemTotalCents: number;
  itemTotalDisplay: string;
  shipmentTotalCents: number;
  shipmentTotalDisplay: string;
  lineItems: LineItemDetail[];
  payments: PaymentDetail[];
  shipments: ShipmentDetail[];
  history: OrderHistoryDetail[];
  shippingAddress?: AddressDetail;
  billingAddress?: AddressDetail;
}

import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'

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
