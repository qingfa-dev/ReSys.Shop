import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface StockAdjustmentRequest {
  quantity: number;
  type: number; // 0: Adjustment, 1: Purchase, 2: Sale, 3: Return, 4: Transfer, 5: Loss, 6: Audit
  reason?: string;
  reference?: string;
}

export interface StockAuditRequest {
  physicalCount: number;
  reason?: string;
  reference?: string;
}

export interface CreateStockLocationRequest {
  name: string;
  presentation?: string;
  code: string;
  type: number;
  isDefault: boolean;
  address: {
    address1: string;
    address2?: string;
    city: string;
    zipCode: string;
    countryCode: string;
    stateCode?: string;
    phone?: string;
    firstName?: string;
    lastName?: string;
    company?: string;
  };
}

export interface CreateStockTransferRequest {
  sourceLocationId: string;
  destinationLocationId: string;
  reason?: string;
}

export interface InventorySearchParams extends ServerQueryingParameters {
  lowStock?: boolean
}

export interface InventoryUnitSearchParams extends InventorySearchParams {
  stockItemId?: string;
  orderId?: string;
  shipmentId?: string;
  state?: number;
}

export interface StockMovementSearchParams extends InventorySearchParams {
  stockItemId?: string;
  type?: number;
}
