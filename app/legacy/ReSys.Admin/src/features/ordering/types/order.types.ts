export interface OrderListItem {
  id: string;
  number: string;
  state: string;
  currency: string;
  total_cents: number;
  total_display: string;
  email?: string;
  created_at: string;
}

export interface AddressDetail {
  id: string;
  firstname: string;
  lastname: string;
  address1: string;
  address2?: string;
  city: string;
  zipcode: string;
  country_code: string;
  state_code?: string;
  phone?: string;
  company?: string;
}

export interface LineItemDetail {
  id: string;
  variant_id: string;
  name: string;
  sku: string;
  quantity: number;
  unit_price_cents: number;
  unit_price_display: string;
  total_cents: number;
  total_display: string;
  inventory_units: InventoryUnitDetail[];
}

export interface PaymentDetail {
  id: string;
  amount_cents: number;
  amount_display: string;
  state: string;
  method_type: string;
  transaction_id?: string;
  created_at: string;
}

export interface InventoryUnitDetail {
  id: string;
  sku: string;
  state: string;
  serial_number?: string;
  pending: boolean;
}

export interface ShipmentDetail {
  id: string;
  number: string;
  state: string;
  tracking_number?: string;
  stock_location_id: string;
  stock_location_name?: string;
  units: InventoryUnitDetail[];
}

export interface OrderHistoryDetail {
  description: string;
  from_state?: string;
  to_state: string;
  triggered_by?: string;
  created_at: string;
  context: Record<string, any>;
}

export interface OrderDetail extends OrderListItem {
  item_total_cents: number;
  item_total_display: string;
  shipment_total_cents: number;
  shipment_total_display: string;
  line_items: LineItemDetail[];
  payments: PaymentDetail[];
  shipments: ShipmentDetail[];
  history: OrderHistoryDetail[];
  shipping_address?: AddressDetail;
  billing_address?: AddressDetail;
}

export interface OrderSearchParams {
  page?: number;
  page_size?: number;
  search?: string;
  filter?: string;
  state?: string;
  sort_by?: string;
  is_descending?: boolean;
  store_id?: string;
  warehouse_id?: string;
  from_date?: string;
  to_date?: string;
}

export interface CreateOrderRequest {
  email: string;
  currency?: string;
  line_items: Array<{ variant_id: string; quantity: number }>;
}

export interface AddOrderItemRequest {
  variant_id: string;
  quantity: number;
}

export interface UpdateAddressesRequest {
  shipping_address?: Partial<AddressDetail>;
  billing_address?: Partial<AddressDetail>;
}

export interface CancelOrderRequest {
  reason?: string;
}

export interface CreateShipmentRequest {
  stock_location_id: string;
  inventory_unit_ids: string[];
}

export interface RefundPaymentRequest {
  amount_cents: number;
  reason: string;
}