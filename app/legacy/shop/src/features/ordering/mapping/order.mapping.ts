import type { OrderEntity, CartEntity, CartItemEntity, AddressEntity } from '../types/entity'
import type { OrderResponse, CartResponse, CartItemResponse, AddressResponse } from '../types/response'
import type { OrderSchemaType, CartItemSchemaType } from '../types/schemas'

export function mapOrderResponseToEntity(response: OrderResponse): OrderEntity {
  return {
    id: response.id,
    orderNumber: response.orderNumber,
    status: response.status as OrderEntity['status'],
    items: response.items.map((item) => ({
      id: item.id,
      productId: item.productId,
      productName: item.productName,
      productImage: item.productImage,
      variantName: item.variantName,
      quantity: item.quantity,
      price: item.price,
    })),
    shippingAddress: mapAddressResponseToEntity(response.shippingAddress),
    billingAddress: mapAddressResponseToEntity(response.billingAddress),
    subtotal: response.subtotal,
    tax: response.tax,
    shipping: response.shipping,
    discount: response.discount,
    total: response.total,
    currency: response.currency,
    createdAt: response.createdAt,
    updatedAt: response.updatedAt,
    trackingNumber: response.trackingNumber,
  }
}

export function mapCartResponseToEntity(response: CartResponse): CartEntity {
  return {
    id: response.id,
    items: response.items.map(mapCartItemResponseToEntity),
    subtotal: response.subtotal,
    tax: response.tax,
    shipping: response.shipping,
    discount: response.discount,
    total: response.total,
    currency: response.currency,
  }
}

export function mapCartItemResponseToEntity(response: CartItemResponse): CartItemEntity {
  return {
    id: response.id,
    productId: response.productId,
    productName: response.productName,
    productImage: response.productImage,
    variantId: response.variantId,
    variantName: response.variantName,
    quantity: response.quantity,
    price: response.price,
    compareAtPrice: response.compareAtPrice,
  }
}

export function mapAddressResponseToEntity(response: AddressResponse): AddressEntity {
  return {
    id: response.id,
    firstName: response.firstName,
    lastName: response.lastName,
    address1: response.address1,
    address2: response.address2,
    city: response.city,
    state: response.state,
    postalCode: response.postalCode,
    country: response.country,
    phone: response.phone,
    isDefault: response.isDefault,
  }
}

export function mapSchemaToOrderEntity(schema: OrderSchemaType): OrderEntity {
  return {
    id: schema.id,
    orderNumber: schema.orderNumber,
    status: schema.status as OrderEntity['status'],
    items: schema.items.map((item) => ({
      id: item.id,
      productId: item.productId,
      productName: item.productName,
      productImage: item.productImage,
      variantName: item.variantName,
      quantity: item.quantity,
      price: item.price,
    })),
    shippingAddress: schema.shippingAddress,
    billingAddress: schema.billingAddress,
    subtotal: schema.subtotal,
    tax: schema.tax,
    shipping: schema.shipping,
    discount: schema.discount,
    total: schema.total,
    currency: schema.currency,
    createdAt: schema.createdAt,
    updatedAt: schema.updatedAt,
    trackingNumber: schema.trackingNumber,
  }
}