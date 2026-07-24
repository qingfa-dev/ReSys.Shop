import type { PaymentMethod } from '../../ordering/types/schemas/order.schema'

export const mockPaymentMethods: PaymentMethod[] = [
  { id: 'pm-1', name: 'Credit Card', type: 'card', lastFour: '4242', isDefault: true },
  { id: 'pm-2', name: 'Debit Card', type: 'card', lastFour: '5555', isDefault: false },
  { id: 'pm-3', name: 'PayPal', type: 'paypal', isDefault: false },
  { id: 'pm-4', name: 'Bank Transfer', type: 'bank', isDefault: false },
  { id: 'pm-5', name: 'Gift Card', type: 'card', lastFour: '1234', isDefault: false },
]

export function getPaymentMethodById(id: string): PaymentMethod | undefined {
  return mockPaymentMethods.find(p => p.id === id)
}

export function getDefaultPaymentMethod(): PaymentMethod | undefined {
  return mockPaymentMethods.find(p => p.isDefault)
}