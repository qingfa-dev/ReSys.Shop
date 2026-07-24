import { describe, it, expect } from 'vitest'
import {
  toReturnRequest,
  fromReturnRequest,
  isReturnPending,
  isReturnApproved,
  isReturnRefunded,
  getReturnStatusLabel,
} from '../mapping/returns.mapping'
import { ReturnRequestSchema } from '../types/schemas'

describe('Returns Mapping', () => {
  describe('toReturnRequest', () => {
    it('should convert schema to entity', () => {
      const schema = ReturnRequestSchema.parse({
        id: 'return-1',
        orderId: 'order-1',
        status: 'pending',
        items: [{ orderItemId: 'item-1', quantity: 1, reason: 'defective' }],
        refundAmount: 0,
        refundMethod: 'original',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      })
      const result = toReturnRequest(schema)
      expect(result.id).toBe('return-1')
      expect(result.status).toBe('pending')
    })
  })

  describe('isReturnPending', () => {
    it('should return true for pending status', () => {
      const returnRequest = { id: 'return-1', orderId: 'order-1', status: 'pending' as const, items: [], refundAmount: 0, refundMethod: 'original' as const, createdAt: '', updatedAt: '' }
      expect(isReturnPending(returnRequest)).toBe(true)
    })

    it('should return false for approved status', () => {
      const returnRequest = { id: 'return-1', orderId: 'order-1', status: 'approved' as const, items: [], refundAmount: 0, refundMethod: 'original' as const, createdAt: '', updatedAt: '' }
      expect(isReturnPending(returnRequest)).toBe(false)
    })
  })

  describe('isReturnApproved', () => {
    it('should return true for approved status', () => {
      const returnRequest = { id: 'return-1', orderId: 'order-1', status: 'approved' as const, items: [], refundAmount: 0, refundMethod: 'original' as const, createdAt: '', updatedAt: '' }
      expect(isReturnApproved(returnRequest)).toBe(true)
    })
  })

  describe('isReturnRefunded', () => {
    it('should return true for refunded status', () => {
      const returnRequest = { id: 'return-1', orderId: 'order-1', status: 'refunded' as const, items: [], refundAmount: 100, refundMethod: 'original' as const, createdAt: '', updatedAt: '' }
      expect(isReturnRefunded(returnRequest)).toBe(true)
    })
  })

  describe('getReturnStatusLabel', () => {
    it('should return correct labels', () => {
      expect(getReturnStatusLabel('pending')).toBe('Pending Review')
      expect(getReturnStatusLabel('approved')).toBe('Approved')
      expect(getReturnStatusLabel('rejected')).toBe('Rejected')
      expect(getReturnStatusLabel('received')).toBe('Received')
      expect(getReturnStatusLabel('refunded')).toBe('Refunded')
    })
  })
})