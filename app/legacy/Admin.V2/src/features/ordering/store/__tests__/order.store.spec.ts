import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useOrderStore } from '../order.store'
import { OrderApi } from '../../api'
import type { OrderResponse, CreateOrderRequest } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDelete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockApprove = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockComplete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCancel = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockResume = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  OrderApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDelete,
    approve: mockApprove,
    complete: mockComplete,
    cancel: mockCancel,
    resume: mockResume,
  },
}))

const mockOrder: OrderResponse = {
  id: '1',
  orderNumber: 'ORD-001',
  status: 'Pending',
  customerId: 'c1',
  customerName: 'Test Customer',
  customerEmail: 'test@example.com',
  subtotal: 100,
  total: 110,
  taxTotal: 10,
  shippingTotal: 0,
  currency: 'USD',
  shipAddress: null,
  billAddress: null,
  lineItems: [],
  notes: null,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function pagedResult(overrides: Partial<{ items: OrderResponse[]; totalCount: number }> = {}) {
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

const createPayload: CreateOrderRequest = {
  customerId: 'c1',
  notes: 'Note',
  lineItems: [{ variantId: 'v1', quantity: 1, unitPrice: 100 }],
}

describe('useOrderStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = useOrderStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockOrder], totalCount: 1 }))
      const store = useOrderStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = useOrderStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = useOrderStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockOrder))
      const store = useOrderStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockOrder)
      expect(OrderApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = useOrderStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = useOrderStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('create', () => {
    it('success', async () => {
      mockCreate.mockResolvedValue(successResult(mockOrder))
      const store = useOrderStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockOrder)
      expect(OrderApi.create).toHaveBeenCalledWith(createPayload)
    })

    it('failure', async () => {
      mockCreate.mockResolvedValue(errorResult('Validation failed'))
      const store = useOrderStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Validation failed')
      expect(store.error).toBe('Validation failed')
    })

    it('network error', async () => {
      mockCreate.mockRejectedValue(new Error('Network'))
      const store = useOrderStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to create')
      expect(store.error).toBe('Failed to create')
    })
  })

  describe('update', () => {
    it('success', async () => {
      mockUpdate.mockResolvedValue(successResult(mockOrder))
      const store = useOrderStore()
      const result = await store.update('1', { notes: 'Updated' })
      expect(result.isSuccess).toBe(true)
      expect(OrderApi.update).toHaveBeenCalledWith('1', { notes: 'Updated' })
    })

    it('failure', async () => {
      mockUpdate.mockResolvedValue(errorResult('Order not found'))
      const store = useOrderStore()
      const result = await store.update('1', { notes: null })
      expect(result.isSuccess).toBe(false)
      expect(store.error).toBe('Order not found')
    })

    it('network error', async () => {
      mockUpdate.mockRejectedValue(new Error('Network'))
      const store = useOrderStore()
      const result = await store.update('1', {})
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to update')
    })
  })

  describe('delete', () => {
    it('success', async () => {
      mockDelete.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useOrderStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(true)
      expect(OrderApi.delete).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDelete.mockResolvedValue(errorResult('Cannot delete completed order'))
      const store = useOrderStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot delete completed order')
    })

    it('network error', async () => {
      mockDelete.mockRejectedValue(new Error('Network'))
      const store = useOrderStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to delete')
    })
  })

  describe('lifecycle actions', () => {
    it('approve calls API with order id', async () => {
      mockApprove.mockResolvedValue(successResult(mockOrder))
      const store = useOrderStore()
      const result = await store.approve('1')
      expect(result.isSuccess).toBe(true)
      expect(OrderApi.approve).toHaveBeenCalledWith('1')
    })

    it('approve handles error', async () => {
      mockApprove.mockResolvedValue(errorResult('Cannot approve order in current status'))
      const store = useOrderStore()
      const result = await store.approve('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot approve order in current status')
    })

    it('complete calls API with order id', async () => {
      mockComplete.mockResolvedValue(successResult({ ...mockOrder, status: 'Completed' }))
      const store = useOrderStore()
      const result = await store.complete('1')
      expect(result.isSuccess).toBe(true)
      expect(OrderApi.complete).toHaveBeenCalledWith('1')
    })

    it('complete handles error', async () => {
      mockComplete.mockResolvedValue(errorResult('Cannot complete unapproved order'))
      const store = useOrderStore()
      const result = await store.complete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot complete unapproved order')
    })

    it('cancel calls API with order id', async () => {
      mockCancel.mockResolvedValue(successResult({ ...mockOrder, status: 'Cancelled' }))
      const store = useOrderStore()
      const result = await store.cancel('1')
      expect(result.isSuccess).toBe(true)
      expect(OrderApi.cancel).toHaveBeenCalledWith('1')
    })

    it('cancel handles error', async () => {
      mockCancel.mockResolvedValue(errorResult('Cannot cancel shipped order'))
      const store = useOrderStore()
      const result = await store.cancel('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot cancel shipped order')
    })

    it('resume calls API with order id', async () => {
      mockResume.mockResolvedValue(successResult({ ...mockOrder, status: 'Pending' }))
      const store = useOrderStore()
      const result = await store.resume('1')
      expect(result.isSuccess).toBe(true)
      expect(OrderApi.resume).toHaveBeenCalledWith('1')
    })

    it('resume handles error', async () => {
      mockResume.mockResolvedValue(errorResult('Cannot resume active order'))
      const store = useOrderStore()
      const result = await store.resume('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot resume active order')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = useOrderStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useOrderStore()
      await store.setSort('createdAt', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'createdAt', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useOrderStore()
      await store.setPage(3)
      await store.setSearch('test')
      expect(store.query.search).toEqual({ value: 'test', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useOrderStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'status', operator: 'Equal', value: 'Pending' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'status', operator: 'Equal', value: 'Pending' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useOrderStore()
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
      const store = useOrderStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during getById', async () => {
      let resolver!: (value: unknown) => void
      mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useOrderStore()
      const promise = store.getById('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockOrder))
      await promise
    })

    it('loading is true during lifecycle action', async () => {
      let resolver!: (value: unknown) => void
      mockApprove.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useOrderStore()
      const promise = store.approve('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockOrder))
      await promise
    })
  })
})
