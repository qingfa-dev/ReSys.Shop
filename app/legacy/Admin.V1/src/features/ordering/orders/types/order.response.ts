export interface OrderListItem {
  id: string;
  number: string;
  status: number;
  checkoutState: number;
  currency: string;
  email: string | null;
  itemCount: number;
  itemTotal: number;
  total: number;
  outstandingBalance: number;
  paymentState: number | null;
  shipmentState: number | null;
  createdAtUtc: string;
  userId: string | null;
}

export interface OrderDetail {
  id: string;
  number: string;
  status: number;
  checkoutState: number;
  currency: string;
  email: string | null;
  specialInstructions: string | null;
  billAddressId: string | null;
  shipAddressId: string | null;
  shippingMethodId: string | null;
  itemTotal: number;
  adjustmentTotal: number;
  shipmentTotal: number;
  total: number;
  paymentTotal: number;
  outstandingBalance: number;
  paymentState: number | null;
  shipmentState: number | null;
  userId: string | null;
  itemCount: number;
  approvedById: string | null;
  approvedAtUtc: string | null;
  completedAtUtc: string | null;
  canceledAtUtc: string | null;
  createdAtUtc: string;
  modifiedAtUtc: string | null;
}
