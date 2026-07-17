export interface StockLocation {
  id: string;
  name: string;
  code: string;
  active: boolean;
  isDefault: boolean;
  type: string;
  city: string;
  countryCode: string;
  position?: number;
  backorderableDefault?: boolean;
  propagateAllVariants?: boolean;
  lowStockThreshold?: number;
  notifyOnLowStock?: boolean;
}

export interface StockLocationDetail extends StockLocation {
  presentation: string | null;
  address: {
    address1: string;
    address2: string | null;
    city: string;
    zipCode: string;
    countryCode: string;
    stateCode: string | null;
    phone: string | null;
    firstName: string | null;
    lastName: string | null;
    company: string | null;
  };
  publicMetadata: Record<string, any>;
  privateMetadata: Record<string, any>;
}

export interface StockItem {
  id: string;
  variantId: string;
  sku: string;
  variantName: string;
  stockLocationId: string;
  stockLocationName: string;
  countOnHand: number;
  quantityReserved?: number;
  countAvailable?: number;
  backorderable: boolean;
}

export interface StockItemDetail extends StockItem {
  backorderLimit: number;
  createdAtUtc: string;
  modifiedAtUtc: string | null;
}

export type ReservationState = 'Reserved' | 'Fulfilled' | 'Released' | 'Expired';

export interface InventoryUnit {
  id: string;
  stockItemId: string;
  sku: string;
  serialNumber: string | null;
  state: ReservationState;
  orderId: string | null;
  shipmentId: string | null;
  createdAtUtc: string;
}

export type TransferState = 'Draft' | 'InTransit' | 'Received' | 'Canceled';

export interface StockTransfer {
  id: string;
  number: string;
  referenceNumber: string;
  sourceLocationId: string;
  sourceLocationName: string;
  destinationLocationId: string;
  destinationLocationName: string;
  state: TransferState;
  createdAtUtc: string;
}

export interface StockTransferItem {
  variantId: string;
  sku: string;
  variantName: string;
  quantity: number;
}

export interface StockTransferDetail extends StockTransfer {
  reason: string | null;
  items: StockTransferItem[];
}

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

import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'

export interface InventorySearchParams extends ServerQueryingParameters {
  lowStock?: boolean
}

export interface InventoryUnitSearchParams extends InventorySearchParams {

  stockItemId?: string;

  orderId?: string;

  shipmentId?: string;

  state?: number;

}



export interface StockMovement {

  id: string;

  stockItemId: string;

  action: string;

  quantity: number;

  previousCountOnHand: number;

  reason: string | null;

  reference: string | null;

  createdAtUtc: string;

  createdBy: string | null;

}



export interface StockMovementSearchParams extends InventorySearchParams {

  stockItemId?: string;

  type?: number;

}
