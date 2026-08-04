import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useShippingMethodStore } from '../shipping-method.store'
import { ShippingMethodApi } from '../../api'
import type { ShippingMethodResponse, CreateShippingMethodRequest, UpdateShippingMethodRequest } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDelete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockActivate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDeactivate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  ShippingMethodApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDelete,
    activate: mockActivate,
    deactivate: mockDeactivate,
  },
}))

const mockMethod: ShippingMethodResponse = {
  id: '1',
  name: 'Express Shipping',
  code: 'EXPRESS',
  description: 'Fast delivery',
  isActive: true,
  displayOrder: 1,
  estimatedDeliveryMin: 1,
  estimatedDeliveryMax: 3,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function pagedResult(overrides: Partial<{ items: ShippingMethodResponse[]; totalCount: number }> = {}) {
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

const createPayload: CreateShippingMethodRequest = {
  name: 'Express Shipping',
  code: 'EXPRESS',
  description: 'Fast delivery',
  isActive: true,
  displayOrder: 1,
  estimatedDeliveryMin: 1,
  estimatedDeliveryMax: 3,
}

const updatePayload: UpdateShippingMethodRequest = {
  name: 'Express Shipping Updated',
  code: 'EXPRESS',
}

describe('useShippingMethodStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = useShippingMethodStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockMethod], totalCount: 1 }))
      const store = useShippingMethodStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = useShippingMethodStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockMethod))
      const store = useShippingMethodStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockMethod)
      expect(ShippingMethodApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = useShippingMethodStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('create', () => {
    it('success', async () => {
      mockCreate.mockResolvedValue(successResult(mockMethod))
      const store = useShippingMethodStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockMethod)
      expect(ShippingMethodApi.create).toHaveBeenCalledWith(createPayload)
    })

    it('failure', async () => {
      mockCreate.mockResolvedValue(errorResult('Validation failed'))
      const store = useShippingMethodStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Validation failed')
      expect(store.error).toBe('Validation failed')
    })

    it('network error', async () => {
      mockCreate.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to create')
      expect(store.error).toBe('Failed to create')
    })
  })

  describe('update', () => {
    it('success', async () => {
      mockUpdate.mockResolvedValue(successResult(mockMethod))
      const store = useShippingMethodStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(true)
      expect(ShippingMethodApi.update).toHaveBeenCalledWith('1', updatePayload)
    })

    it('failure', async () => {
      mockUpdate.mockResolvedValue(errorResult('Not found'))
      const store = useShippingMethodStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockUpdate.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to update')
    })
  })

  describe('delete', () => {
    it('success', async () => {
      mockDelete.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useShippingMethodStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(true)
      expect(ShippingMethodApi.delete).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDelete.mockResolvedValue(errorResult('Cannot delete'))
      const store = useShippingMethodStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot delete')
    })

    it('network error', async () => {
      mockDelete.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to delete')
    })
  })

  describe('activate', () => {
    it('success', async () => {
      mockActivate.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useShippingMethodStore()
      const result = await store.activate('1')
      expect(result.isSuccess).toBe(true)
      expect(ShippingMethodApi.activate).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockActivate.mockResolvedValue(errorResult('Cannot activate'))
      const store = useShippingMethodStore()
      const result = await store.activate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot activate')
    })

    it('network error', async () => {
      mockActivate.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      const result = await store.activate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to activate')
    })
  })

  describe('deactivate', () => {
    it('success', async () => {
      mockDeactivate.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useShippingMethodStore()
      const result = await store.deactivate('1')
      expect(result.isSuccess).toBe(true)
      expect(ShippingMethodApi.deactivate).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDeactivate.mockResolvedValue(errorResult('Cannot deactivate'))
      const store = useShippingMethodStore()
      const result = await store.deactivate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot deactivate')
    })

    it('network error', async () => {
      mockDeactivate.mockRejectedValue(new Error('Network'))
      const store = useShippingMethodStore()
      const result = await store.deactivate('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to deactivate')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = useShippingMethodStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingMethodStore()
      await store.setSort('name', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'name', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingMethodStore()
      await store.setPage(3)
      await store.setSearch('express')
      expect(store.query.search).toEqual({ value: 'express', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingMethodStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'isActive', operator: 'Equal', value: 'true' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'isActive', operator: 'Equal', value: 'true' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useShippingMethodStore()
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
      const store = useShippingMethodStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during getById', async () => {
      let resolver!: (value: unknown) => void
      mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useShippingMethodStore()
      const promise = store.getById('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockMethod))
      await promise
    })

    it('loading is true during activate', async () => {
      let resolver!: (value: unknown) => void
      mockActivate.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useShippingMethodStore()
      const promise = store.activate('1')
      expect(store.loading).toBe(true)
      resolver({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      await promise
    })
  })
})
