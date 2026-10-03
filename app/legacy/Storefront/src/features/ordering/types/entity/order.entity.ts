export type CartItemEntity = import('../schemas/order.schema').CartItem
export type CartEntity = import('../schemas/order.schema').Cart & { id: string; tax: number; shipping: number; currency: string }
export type OrderItemEntity = import('../schemas/order.schema').OrderItem
export type OrderEntity = import('../schemas/order.schema').Order
export type OrderStatusEntity = import('../schemas/order.schema').OrderStatus
export type AddressEntity = import('../schemas/order.schema').Address
export type ShippingMethodEntity = import('../schemas/order.schema').ShippingMethod
export type PaymentMethodEntity = import('../schemas/order.schema').PaymentMethod