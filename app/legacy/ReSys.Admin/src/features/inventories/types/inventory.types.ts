export interface StockLocation {
  id: string;
  name: string;
  code: string;
  active: boolean;
  is_default: boolean;
  type: string;
  city: string;
  country_code: string;
}

export interface StockLocationDetail extends StockLocation {
  presentation: string | null;
  address: {
    address1: string;
    address2: string | null;
    city: string;
    zip_code: string;
    country_code: string;
    state_code: string | null;
    phone: string | null;
    first_name: string | null;
    last_name: string | null;
    company: string | null;
  };
  public_metadata: Record<string, any>;
  private_metadata: Record<string, any>;
}

export interface StockItem {
  id: string;
  variant_id: string;
  sku: string;
  variant_name: string;
  stock_location_id: string;
  stock_location_name: string;
  quantity_on_hand: number;
  quantity_reserved: number;
  count_available: number;
  backorderable: boolean;
}

export interface StockItemDetail extends StockItem {
  backorder_limit: number;
  created_at: string;
  updated_at: string | null;
}

export interface InventoryUnit {
  id: string;
  stock_item_id: string;
  sku: string;
  serial_number: string | null;
  state: 'Available' | 'Reserved' | 'Shipped' | 'Damaged' | 'Returned' | 'Sold';
  order_id: string | null;
  shipment_id: string | null;
  created_at: string;
}

export interface StockTransfer {
  id: string;
  reference_number: string;
  source_location_id: string;
  source_location_name: string;
  destination_location_id: string;
  destination_location_name: string;
  status: 'Pending' | 'Shipped' | 'Received' | 'Canceled';
  created_at: string;
}

export interface StockTransferItem {
  variant_id: string;
  sku: string;
  variant_name: string;
  quantity: number;
}

export interface StockTransferDetail extends StockTransfer {
  reason: string | null;
  items: StockTransferItem[];
}

export interface StockAdjustmentRequest {
  quantity: number;
  type: number; // 0: Adjustment, 1: Purchase, 2: Sale, 3: Return, 4: Transfer, 5: Loss, 6: Audit
  unit_cost: number;
  reason?: string;
  reference?: string;
}

export interface StockAuditRequest {
  physical_count: number;
  reason?: string;
  reference?: string;
}

export interface CreateStockLocationRequest {
  name: string;
  presentation?: string;
  code: string;
  type: number;
  is_default: boolean;
  address: {
    address1: string;
    address2?: string;
    city: string;
    zip_code: string;
    country_code: string;
    state_code?: string;
    phone?: string;
    first_name?: string;
    last_name?: string;
    company?: string;
  };
}

export interface CreateStockTransferRequest {
  source_location_id: string;
  destination_location_id: string;
  reason?: string;
}

export interface InventorySearchParams {
  page?: number;
  page_size?: number;
  search?: string;
  filter?: string;
  sort_by?: string;
  is_descending?: boolean;
  low_stock?: boolean;
}

export interface InventoryUnitSearchParams extends InventorySearchParams {

  stock_item_id?: string;

  order_id?: string;

  shipment_id?: string;

  state?: number;

}



export interface StockMovement {

  id: string;

  stock_item_id: string;

  type: string;

  quantity: number;

  balance_before: number;

  balance_after: number;

  unit_cost: number;

  reason: string | null;

  reference: string | null;

  created_at: string;

  created_by: string | null;

}



export interface StockMovementSearchParams extends InventorySearchParams {

  stock_item_id?: string;

  type?: number;

}
