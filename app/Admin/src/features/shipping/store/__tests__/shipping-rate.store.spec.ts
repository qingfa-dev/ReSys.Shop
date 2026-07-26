import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useShippingRateStore } from '../shipping-rate.store'
import { ShippingRateApi } from '../../api'
import type { ShippingRateResponse, CreateShippingRateRequest, UpdateShippingRateRequest } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDelete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  ShippingRateApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDelete,
  },
}))

const mockRate: ShippingRateResponse = {
  id: '1',
  shippingMethodId: 'm1',
  shippingMethodName: 'Express Shipping',
  name: 'Standard Rate',
  rate: 5.99,
  currency: 'USD',
  minOrderAmount: 0,
  maxOrderAmount: 100,
  minWeight: 0,
  maxWeight: 10,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function pagedResult(overrides: Partial<{ items: ShippingRateResponse[]; totalCount: number }> = {}) {
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

const createPayload: CreateShippingRateRequest = {
  shippingMethodId: 'm1',
  name: 'Standard Rate',
  rate: 5.99,
  currency: 'USD',
  minOrderAmount: 0,
  maxOrderAmount: 100,
  minWeight: 0,
  maxWeight: 10,
}

const updatePayload: UpdateShippingRateRequest = {
  shippingMethodId: 'm1',
  name: 'Updated Rate',
  rate: 7.99,
  currency: 'USD',
}

describe('useShippingRateStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = useShippingRateStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockRate], totalCount: 1 }))
      const store = useShippingRateStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = useShippingRateStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = useShippingRateStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockRate))
      const store = useShippingRateStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockRate)
      expect(ShippingRateApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = useShippingRateStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = useShippingRateStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('create', () => {
    it('success', async () => {
      mockCreate.mockResolvedValue(successResult(mockRate))
      const store = useShippingRateStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockRate)
      expect(ShippingRateApi.create).toHaveBeenCalledWith(createPayload)
    })

    it('creates with weight range fields', async () => {
      mockCreate.mockResolvedValue(successResult(mockRate))
      const store = useShippingRateStore()
      const payloadWithWeight = { ...createPayload, minWeight: 1, maxWeight: 5 }
      await store.create(payloadWithWeight)
      expect(ShippingRateApi.create).toHaveBeenCalledWith(payloadWithWeight)
    })

    it('creates with currency field', async () => {
      mockCreate.mockResolvedValue(successResult({ ...mockRate, currency: 'EUR' }))
      const store = useShippingRateStore()
      const payloadEur = { ...createPayload, currency: 'EUR' }
      const result = await store.create(payloadEur)
      expect(ShippingRateApi.create).toHaveBeenCalledWith(payloadEur)
      expect(result.value?.currency).toBe('EUR')
    })

    it('failure', async () => {
      mockCreate.mockResolvedValue(errorResult('Validation failed'))
      const store = useShippingRateStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Validation failed')
      expect(store.error).toBe('Validation failed')
    })

    it('network error', async () => {
      mockCreate.mockRejectedValue(new Error('Network'))
      const store = useShippingRateStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to create')
      expect(store.error).toBe('Failed to create')
    })
  })

  describe('update', () => {
    it('success', async () => {
      mockUpdate.mockResolvedValue(successResult(mockRate))
      const store = useShippingRateStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(true)
      expect(ShippingRateApi.update).toHaveBeenCalledWith('1', updatePayload)
    })

    it('failure', async () => {
      mockUpdate.mockResolvedValue(errorResult('Not found'))
      const store = useShippingRateStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockUpdate.mockRejectedValue(new Error('Network'))
      const store = useShippingRateStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to update')
    })
  })

  describe('delete', () => {
    it('success', async () => {
      mockDelete.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useShippingRateStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(true)
      expect(ShippingRateApi.delete).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDelete.mockResolvedValue(errorResult('Cannot delete'))
      const store = useShippingRateStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot delete')
    })

    it('network error', async () => {
      mockDelete.mockRejectedValue(new Error('Network'))
      const store = useShippingRateStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to delete')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = useShippingRateStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingRateStore()
      await store.setSort('rate', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'rate', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingRateStore()
      await store.setPage(3)
      await store.setSearch('standard')
      expect(store.query.search).toEqual({ value: 'standard', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingRateStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'currency', operator: 'Equal', value: 'USD' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'currency', operator: 'Equal', value: 'USD' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingRateStore()
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
      const store = useShippingRateStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during getById', async () => {
      let resolver!: (value: unknown) => void
      mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useShippingRateStore()
      const promise = store.getById('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockRate))
      await promise
    })

    it('loading is true during create', async () => {
      let resolver!: (value: unknown) => void
      mockCreate.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useShippingRateStore()
      const promise = store.create(createPayload)
      expect(store.loading).toBe(true)
      resolver(successResult(mockRate))
      await promise
    })
  })
})
