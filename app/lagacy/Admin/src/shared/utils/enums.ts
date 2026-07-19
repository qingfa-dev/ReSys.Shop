export const ProductStatusMap: Record<number, string> = {
  0: 'Draft',
  1: 'Active',
  2: 'Archived',
}

export const OrderStatusMap: Record<number, string> = {
  0: 'Draft',
  1: 'Placed',
  2: 'Canceled',
  4: 'Expired',
}

export const CheckoutStateMap: Record<number, string> = {
  0: 'Cart',
  1: 'Address',
  2: 'Delivery',
  4: 'Payment',
  8: 'Confirm',
  16: 'Complete',
}

export const PaymentStateMap: Record<number, string> = {
  0: 'Pending',
  1: 'Completed',
  2: 'Failed',
  4: 'Voided',
  8: 'Refunded',
}

export const ShipmentStateMap: Record<number, string> = {
  0: 'Pending',
  1: 'Ready',
  2: 'Shipped',
  4: 'Delivered',
  8: 'Canceled',
  16: 'Returned',
}

export const TransferStateMap: Record<number, string> = {
  0: 'Pending',
  1: 'Approved',
  2: 'InTransit',
  4: 'Completed',
  8: 'Canceled',
}

export const ReservationStateMap: Record<number, string> = {
  0: 'Reserved',
  1: 'Picked',
  2: 'Shipped',
  4: 'Cancelled',
  8: 'Expired',
}
