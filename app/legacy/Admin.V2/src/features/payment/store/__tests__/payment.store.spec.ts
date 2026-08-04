import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { usePaymentStore } from '../payment.store'
import { PaymentApi } from '../../api'
import type { PaymentResponse } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCapture = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockVoid = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockRefund = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  PaymentApi: {
    getMany: mockGetMany,
    get: mockGet,
    capture: mockCapture,
    void: mockVoid,
    refund: mockRefund,
  },
}))

const mockPayment: PaymentResponse = {
  id: '1',
  orderId: 'o1',
  orderNumber: 'ORD-001',
  paymentMethodId: 'pm1',
  paymentMethodName: 'Credit Card',
  amount: 110,
  currency: 'USD',
  status: 'Authorized',
  authorizationCode: 'AUTH-001',
  capturedAt: null,
  voidedAt: null,
  refundedAt: null,
  notes: null,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function pagedResult(overrides: Partial<{ items: PaymentResponse[]; totalCount: number }> = {}) {
  return {
    isSuccess: true,
    statusCode: 200,
    items: overrides.items ?? [],
    page: 1,
    pageSize: 20,
    totalCount: overrides.totalCount ?? 0,
    errors: [],
    message: null,
    metadata: null,
  }
}

function successResult<T>(value: T): Result<T> {
  return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value }
}

function errorResult(message = 'Something went wrong'): Result<any> {
  return { isSuccess: false, statusCode: 400, errors: [], message, metadata: null, value: null }
}

describe('usePaymentStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = usePaymentStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockPayment], totalCount: 1 }))
      const store = usePaymentStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = usePaymentStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = usePaymentStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockPayment))
      const store = usePaymentStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockPayment)
      expect(PaymentApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = usePaymentStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = usePaymentStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('capture', () => {
    it('success', async () => {
      const captured = { ...mockPayment, status: 'Captured', capturedAt: '2026-01-02T00:00:00Z' }
      mockCapture.mockResolvedValue(successResult(captured))
      const store = usePaymentStore()
      const result = await store.capture('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value.status).toBe('Captured')
      expect(PaymentApi.capture).toHaveBeenCalledWith('1', undefined)
    })

    it('success with amount', async () => {
      mockCapture.mockResolvedValue(successResult(mockPayment))
      const store = usePaymentStore()
      await store.capture('1', { amount: 50 })
      expect(PaymentApi.capture).toHaveBeenCalledWith('1', { amount: 50 })
    })

    it('failure', async () => {
      mockCapture.mockResolvedValue(errorResult('Cannot capture, already captured'))
      const store = usePaymentStore()
      const result = await store.capture('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot capture, already captured')
      expect(store.error).toBe('Cannot capture, already captured')
    })

    it('network error', async () => {
      mockCapture.mockRejectedValue(new Error('Network'))
      const store = usePaymentStore()
      const result = await store.capture('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to capture')
      expect(store.error).toBe('Failed to capture')
    })
  })

  describe('void', () => {
    it('success', async () => {
      const voided = { ...mockPayment, status: 'Voided', voidedAt: '2026-01-02T00:00:00Z' }
      mockVoid.mockResolvedValue(successResult(voided))
      const store = usePaymentStore()
      const result = await store.void('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value.status).toBe('Voided')
      expect(PaymentApi.void).toHaveBeenCalledWith('1', undefined)
    })

    it('success with reason', async () => {
      mockVoid.mockResolvedValue(successResult(mockPayment))
      const store = usePaymentStore()
      await store.void('1', { reason: 'Duplicate' })
      expect(PaymentApi.void).toHaveBeenCalledWith('1', { reason: 'Duplicate' })
    })

    it('failure', async () => {
      mockVoid.mockResolvedValue(errorResult('Cannot void, already settled'))
      const store = usePaymentStore()
      const result = await store.void('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot void, already settled')
      expect(store.error).toBe('Cannot void, already settled')
    })

    it('network error', async () => {
      mockVoid.mockRejectedValue(new Error('Network'))
      const store = usePaymentStore()
      const result = await store.void('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to void')
      expect(store.error).toBe('Failed to void')
    })
  })

  describe('refund', () => {
    it('success', async () => {
      const refunded = { ...mockPayment, status: 'Refunded', refundedAt: '2026-01-02T00:00:00Z' }
      mockRefund.mockResolvedValue(successResult(refunded))
      const store = usePaymentStore()
      const result = await store.refund('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value.status).toBe('Refunded')
      expect(PaymentApi.refund).toHaveBeenCalledWith('1', undefined)
    })

    it('success with amount and reason', async () => {
      mockRefund.mockResolvedValue(successResult(mockPayment))
      const store = usePaymentStore()
      await store.refund('1', { amount: 30, reason: 'Partial refund' })
      expect(PaymentApi.refund).toHaveBeenCalledWith('1', { amount: 30, reason: 'Partial refund' })
    })

    it('failure', async () => {
      mockRefund.mockResolvedValue(errorResult('Cannot refund, not captured'))
      const store = usePaymentStore()
      const result = await store.refund('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot refund, not captured')
      expect(store.error).toBe('Cannot refund, not captured')
    })

    it('network error', async () => {
      mockRefund.mockRejectedValue(new Error('Network'))
      const store = usePaymentStore()
      const result = await store.refund('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to refund')
      expect(store.error).toBe('Failed to refund')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = usePaymentStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentStore()
      await store.setSort('amount', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'amount', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentStore()
      await store.setPage(3)
      await store.setSearch('ORD-001')
      expect(store.query.search).toEqual({ value: 'ORD-001', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'status', operator: 'Equal', value: 'Authorized' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'status', operator: 'Equal', value: 'Authorized' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentStore()
      await store.setPage(5)
      await store.resetQuery()
      expect(store.query.page).toBe(1)
      expect(store.query.search).toBeUndefined()
      expect(store.query.sort).toEqual([{ field: 'createdAt', direction: 'Descending' }])
      expect(mockGetMany).toHaveBeenCalled()
    })
  })

  describe('loading state', () => {
    it('loading is true during fetchMany', async () => {
      let resolver!: (value: unknown) => void
      mockGetMany.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = usePaymentStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during getById', async () => {
      let resolver!: (value: unknown) => void
      mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = usePaymentStore()
      const promise = store.getById('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockPayment))
      await promise
    })

    it('loading is true during capture', async () => {
      let resolver!: (value: unknown) => void
      mockCapture.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = usePaymentStore()
      const promise = store.capture('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockPayment))
      await promise
    })
  })
})
