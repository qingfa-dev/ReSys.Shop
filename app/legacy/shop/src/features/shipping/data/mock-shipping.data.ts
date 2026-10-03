import type { ShippingMethod } from '../../ordering/types/schemas/order.schema'

export const mockShippingMethods: ShippingMethod[] = [
  { id: 'ship-1', name: 'Standard Shipping', price: 5.99, estimatedDays: 5, carrier: 'USPS' },
  { id: 'ship-2', name: 'Express Shipping', price: 12.99, estimatedDays: 2, carrier: 'FedEx' },
  { id: 'ship-3', name: 'Overnight Shipping', price: 24.99, estimatedDays: 1, carrier: 'UPS' },
  { id: 'ship-4', name: 'Free Shipping', price: 0, estimatedDays: 7, carrier: 'USPS' },
  { id: 'ship-5', name: 'International Shipping', price: 29.99, estimatedDays: 14, carrier: 'DHL' },
]

export function getShippingMethodById(id: string): ShippingMethod | undefined {
  return mockShippingMethods.find(s => s.id === id)
}

export function getFreeShippingMethod(): ShippingMethod | undefined {
  return mockShippingMethods.find(s => s.price === 0)
}