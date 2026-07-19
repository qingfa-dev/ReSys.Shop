import { describe, it, expect } from 'vitest'
import { mapOrderListItem, mapOrderDetail } from '../mappers/order.mapper'

describe('mapOrderListItem', () => {
  const dto = {
    id: 'o1', number: 'R100001', status: 1, checkoutState: 4,
    currency: 'USD', email: 'a@b.com', itemCount: 2,
    itemTotal: 59.98, total: 59.98, outstandingBalance: 59.98,
    paymentState: 1, shipmentState: null,
    createdAtUtc: '2025-01-01T00:00:00Z', userId: null, storeId: null,
  }
  it('maps and computes display fields', () => {
    const result = mapOrderListItem(dto)
    expect(result.number).toBe('R100001')
    expect(result.totalDisplay).toBe('$59.98')
    expect(result.statusLabel).toBe('Placed')
    expect(result.paymentStateLabel).toBe('Completed')
    expect(result.shipmentStateLabel).toBeNull()
  })
  it('handles zero values', () => {
    const zero = { ...dto, total: 0, itemTotal: 0, outstandingBalance: 0 }
    const result = mapOrderListItem(zero)
    expect(result.totalDisplay).toBe('$0.00')
  })
})

describe('mapOrderDetail', () => {
  it('includes all display fields', () => {
    const dto = {
      id: 'o1', number: 'R1', status: 0, checkoutState: 0,
      currency: 'USD', email: null, specialInstructions: null,
      billAddressId: null, shipAddressId: null, shippingMethodId: null,
      itemTotal: 25.00, adjustmentTotal: 0, shipmentTotal: 5.00,
      total: 30.00, paymentTotal: 0, outstandingBalance: 30.00,
      paymentState: null, shipmentState: null,
      userId: null, storeId: null, itemCount: 1,
      approvedById: null, approvedAtUtc: null,
      completedAtUtc: null, canceledAtUtc: null,
      createdAtUtc: '', modifiedAtUtc: null,
    }
    const result = mapOrderDetail(dto)
    expect(result.statusLabel).toBe('Draft')
    expect(result.totalDisplay).toBe('$30.00')
    expect(result.itemTotalDisplay).toBe('$25.00')
    expect(result.shipmentTotalDisplay).toBe('$5.00')
  })
})
