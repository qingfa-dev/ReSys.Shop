import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { usePaymentMethodStore } from '../payment-method.store'
import { PaymentMethodApi } from '../../api'
import type { PaymentMethodResponse, CreatePaymentMethodRequest } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDelete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockActivate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDeactivate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  PaymentMethodApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDelete,
    activate: mockActivate,
    deactivate: mockDeactivate,
  },
}))

const mockPaymentMethod: PaymentMethodResponse = {
  id: '1',
  name: 'Credit Card',
  code: 'credit_card',
  description: 'Pay with credit card',
  isActive: true,
  isTestMode: false,
  displayOrder: 1,
  supportedCurrencies: 'USD,EUR',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

const createPayload: CreatePaymentMethodRequest = {
  name: 'New Method',
  code: 'new_code',
  description: 'A new payment method',
  isActive: true,
  isTestMode: false,
  displayOrder: 2,
  supportedCurrencies: 'USD',
}

const updatePayload = {
  name: 'Updated Method',
  code: 'updated_code',
  description: 'Updated description',
  isActive: true,
  isTestMode: false,
  displayOrder: 2,
  supportedCurrencies: 'USD,EUR',
}

function pagedResult(overrides: Partial<{ items: PaymentMethodResponse[]; totalCount: number }> = {}) {
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

function voidResult(): Result<void> {
  return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
}

function errorResult(message = 'Something went wrong'): Result<any> {
  return { isSuccess: false, statusCode: 400, errors: [], message, metadata: null, value: null }
}

describe('usePaymentMethodStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = usePaymentMethodStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockPaymentMethod], totalCount: 1 }))
      const store = usePaymentMethodStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = usePaymentMethodStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockPaymentMethod))
      const store = usePaymentMethodStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockPaymentMethod)
      expect(PaymentMethodApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = usePaymentMethodStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('create', () => {
    it('success', async () => {
      mockCreate.mockResolvedValue(successResult(mockPaymentMethod))
      const store = usePaymentMethodStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockPaymentMethod)
      expect(PaymentMethodApi.create).toHaveBeenCalledWith(createPayload)
    })

    it('failure', async () => {
      mockCreate.mockResolvedValue(errorResult('Validation failed'))
      const store = usePaymentMethodStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Validation failed')
      expect(store.error).toBe('Validation failed')
    })

    it('network error', async () => {
      mockCreate.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to create')
      expect(store.error).toBe('Failed to create')
    })
  })

  describe('update', () => {
    it('success', async () => {
      mockUpdate.mockResolvedValue(successResult(mockPaymentMethod))
      const store = usePaymentMethodStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(true)
      expect(PaymentMethodApi.update).toHaveBeenCalledWith('1', updatePayload)
    })

    it('failure', async () => {
      mockUpdate.mockResolvedValue(errorResult('Method not found'))
      const store = usePaymentMethodStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(store.error).toBe('Method not found')
    })

    it('network error', async () => {
      mockUpdate.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to update')
    })
  })

  describe('delete', () => {
    it('success', async () => {
      mockDelete.mockResolvedValue(voidResult())
      const store = usePaymentMethodStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(true)
      expect(PaymentMethodApi.delete).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDelete.mockResolvedValue(errorResult('Cannot delete active method'))
      const store = usePaymentMethodStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot delete active method')
    })

    it('network error', async () => {
      mockDelete.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to delete')
    })
  })

  describe('activate', () => {
    it('success', async () => {
      mockActivate.mockResolvedValue(voidResult())
      const store = usePaymentMethodStore()
      const result = await store.activate('1')
      expect(result.isSuccess).toBe(true)
      expect(PaymentMethodApi.activate).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockActivate.mockResolvedValue(errorResult('Method already active'))
      const store = usePaymentMethodStore()
      const result = await store.activate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Method already active')
      expect(store.error).toBe('Method already active')
    })

    it('network error', async () => {
      mockActivate.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      const result = await store.activate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to activate')
      expect(store.error).toBe('Failed to activate')
    })
  })

  describe('deactivate', () => {
    it('success', async () => {
      mockDeactivate.mockResolvedValue(voidResult())
      const store = usePaymentMethodStore()
      const result = await store.deactivate('1')
      expect(result.isSuccess).toBe(true)
      expect(PaymentMethodApi.deactivate).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDeactivate.mockResolvedValue(errorResult('Method already inactive'))
      const store = usePaymentMethodStore()
      const result = await store.deactivate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Method already inactive')
      expect(store.error).toBe('Method already inactive')
    })

    it('network error', async () => {
      mockDeactivate.mockRejectedValue(new Error('Network'))
      const store = usePaymentMethodStore()
      const result = await store.deactivate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to deactivate')
      expect(store.error).toBe('Failed to deactivate')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = usePaymentMethodStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentMethodStore()
      await store.setSort('displayOrder', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'displayOrder', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentMethodStore()
      await store.setPage(3)
      await store.setSearch('Credit')
      expect(store.query.search).toEqual({ value: 'Credit', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentMethodStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'isActive', operator: 'Equal', value: 'true' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'isActive', operator: 'Equal', value: 'true' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = usePaymentMethodStore()
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
      const store = usePaymentMethodStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during create', async () => {
      let resolver!: (value: unknown) => void
      mockCreate.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = usePaymentMethodStore()
      const promise = store.create(createPayload)
      expect(store.loading).toBe(true)
      resolver(successResult(mockPaymentMethod))
      await promise
    })

    it('loading is true during activate', async () => {
      let resolver!: (value: unknown) => void
      mockActivate.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = usePaymentMethodStore()
      const promise = store.activate('1')
      expect(store.loading).toBe(true)
      resolver(voidResult())
      await promise
    })
  })
})
