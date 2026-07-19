import { describe, it, expect } from 'vitest'
import { mapPaymentListItem, mapPaymentDetail } from '../mappers/payment.mapper'

describe('mapPaymentListItem', () => {
  const dto = {
    id: 'p1', orderId: 'o1', amount: 59.98, currency: 'USD',
    status: 1, methodName: 'Credit Card', createdAtUtc: '2025-01-01T00:00:00Z',
  }
  it('maps and computes display fields', () => {
    const result = mapPaymentListItem(dto)
    expect(result.amountDisplay).toBe('$59.98')
    expect(result.statusLabel).toBe('Completed')
    expect(result.id).toBe('p1')
  })
  it('handles zero amount', () => {
    const zero = { ...dto, amount: 0 }
    const result = mapPaymentListItem(zero)
    expect(result.amountDisplay).toBe('$0.00')
  })
})

describe('mapPaymentDetail', () => {
  it('includes display fields with transactions', () => {
    const dto = {
      id: 'p1', orderId: 'o1', amount: 100.00, currency: 'USD',
      status: 1, methodName: 'PayPal', createdAtUtc: '2025-01-01T00:00:00Z',
      gatewayResponse: null, transactions: [],
    }
    const result = mapPaymentDetail(dto)
    expect(result.amountDisplay).toBe('$100.00')
    expect(result.statusLabel).toBe('Completed')
  })
})
