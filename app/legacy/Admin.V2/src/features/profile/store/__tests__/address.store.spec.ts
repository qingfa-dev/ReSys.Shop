import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAddressStore } from '../address.store'
import { AddressApi } from '../../api'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDeleteApi = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  AddressApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDeleteApi,
  },
}))

function pagedResult(overrides: Partial<{ items: any[], totalCount: number }> = {}) {
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

function errorResult(message = 'Something went wrong') {
  return { isSuccess: false, statusCode: 400, value: null, errors: [], message, metadata: null }
}

function singleResult<T>(value: T) {
  return {
    isSuccess: true,
    statusCode: 200,
    value,
    errors: [],
    message: null,
    metadata: null,
  }
}

const mockAddress = {
  id: 'a1',
  firstName: 'John',
  lastName: 'Doe',
  address1: '123 Main St',
  address2: null,
  city: 'New York',
  state: 'NY',
  postalCode: '10001',
  country: 'US',
  phone: null,
  isDefault: true,
  createdAt: '2025-01-01',
  updatedAt: '2025-01-01',
}

describe('useAddressStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('has initial state', () => {
    const store = useAddressStore()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
    expect(store.items).toEqual([])
    expect(store.totalRecords).toBe(0)
    expect(store.query.page).toBe(1)
  })

  it('fetchMany success', async () => {
    mockGetMany.mockResolvedValue(pagedResult({ items: [mockAddress], totalCount: 1 }))
    const store = useAddressStore()
    await store.fetchMany()
    expect(store.loading).toBe(false)
    expect(store.items).toHaveLength(1)
    expect(store.totalRecords).toBe(1)
    expect(store.error).toBeNull()
  })

  it('fetchMany failure', async () => {
    mockGetMany.mockResolvedValue(errorResult('Failed to load'))
    const store = useAddressStore()
    await store.fetchMany()
    expect(store.loading).toBe(false)
    expect(store.error).toBe('Failed to load')
    expect(store.items).toEqual([])
    expect(store.totalRecords).toBe(0)
  })

  it('fetchMany network error', async () => {
    mockGetMany.mockRejectedValue(new Error('Network'))
    const store = useAddressStore()
    await store.fetchMany()
    expect(store.loading).toBe(false)
    expect(store.error).toBe('Failed to load')
    expect(store.items).toEqual([])
    expect(store.totalRecords).toBe(0)
  })

  it('setPage updates query and re-fetches', async () => {
    mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
    const store = useAddressStore()
    await store.setPage(3)
    expect(store.query.page).toBe(3)
    expect(mockGetMany).toHaveBeenCalled()
  })

  it('setSearch sets search and resets page', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useAddressStore()
    await store.setPage(3)
    await store.setSearch('test')
    expect(mockGetMany).toHaveBeenCalled()
    expect(store.query.search).toEqual({ value: 'test', mode: 'Any' })
    expect(store.query.page).toBe(1)
  })

  it('setSort updates sort clause', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useAddressStore()
    await store.setSort('city', 'Ascending')
    expect(store.query.sort).toEqual([{ field: 'city', direction: 'Ascending' }])
  })

  it('setFilter sets filter group and resets page', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useAddressStore()
    await store.setFilter({ logic: 'And', conditions: [{ field: 'city', operator: 'Equal', value: 'New York' }], groups: [] })
    expect(mockGetMany).toHaveBeenCalled()
    expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'city', operator: 'Equal', value: 'New York' }], groups: [] })
    expect(store.query.page).toBe(1)
  })

  it('setFilters builds filter group from configs', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useAddressStore()
    await store.setFilters([{ field: 'country', operator: 'Equal', value: 'US' } as any])
    expect(mockGetMany).toHaveBeenCalled()
    expect(store.query.filters).toBeDefined()
    expect(store.query.filters!.conditions).toHaveLength(1)
    expect(store.query.page).toBe(1)
  })

  it('resetQuery restores defaults', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useAddressStore()
    await store.setPage(5)
    await store.resetQuery()
    expect(store.query.page).toBe(1)
    expect(store.query.search).toBeUndefined()
    expect(store.query.sort).toEqual([{ field: 'createdAt', direction: 'Descending' }])
    expect(mockGetMany).toHaveBeenCalled()
  })

  it('loading is true during fetchMany', async () => {
    let resolver!: (value: unknown) => void
    mockGetMany.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
    const store = useAddressStore()
    const promise = store.fetchMany()
    expect(store.loading).toBe(true)
    resolver(pagedResult())
    await promise
  })

  it('setSearchQuery updates searchQuery ref without re-fetch', () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useAddressStore()
    store.setSearchQuery('test')
    expect(store.searchQuery).toBe('test')
    expect(mockGetMany).not.toHaveBeenCalled()
  })
})

describe('AddressApi CRUD', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('getById success', async () => {
    mockGet.mockResolvedValue(singleResult(mockAddress))
    const res = await AddressApi.get('a1')
    expect(mockGet).toHaveBeenCalledWith('a1')
    expect(res.isSuccess).toBe(true)
    expect(res.value).toEqual(mockAddress)
  })

  it('getById error', async () => {
    mockGet.mockResolvedValue(errorResult('Address not found'))
    const res = await AddressApi.get('nonexistent')
    expect(res.isSuccess).toBe(false)
    expect(res.message).toBe('Address not found')
  })

  it('create success', async () => {
    mockCreate.mockResolvedValue(singleResult(mockAddress))
    const payload = { firstName: 'John', lastName: 'Doe', address1: '123 Main St', city: 'New York', postalCode: '10001', country: 'US' }
    const res = await AddressApi.create(payload)
    expect(mockCreate).toHaveBeenCalledWith(payload)
    expect(res.isSuccess).toBe(true)
    expect(res.value).toEqual(mockAddress)
  })

  it('create error', async () => {
    mockCreate.mockResolvedValue(errorResult('Validation failed'))
    const res = await AddressApi.create({ firstName: '', lastName: '', address1: '', city: '', postalCode: '', country: '' } as any)
    expect(res.isSuccess).toBe(false)
    expect(res.message).toBe('Validation failed')
  })

  it('update success', async () => {
    const updated = { ...mockAddress, city: 'Boston' }
    mockUpdate.mockResolvedValue(singleResult(updated))
    const res = await AddressApi.update('a1', { city: 'Boston' } as any)
    expect(mockUpdate).toHaveBeenCalledWith('a1', { city: 'Boston' })
    expect(res.isSuccess).toBe(true)
    expect(res.value.city).toBe('Boston')
  })

  it('update error', async () => {
    mockUpdate.mockResolvedValue(errorResult('Address not found'))
    const res = await AddressApi.update('nonexistent', { city: 'Boston' } as any)
    expect(res.isSuccess).toBe(false)
    expect(res.message).toBe('Address not found')
  })

  it('delete success', async () => {
    mockDeleteApi.mockResolvedValue({ isSuccess: true, statusCode: 200, value: null, errors: [], message: null, metadata: null })
    const res = await AddressApi.delete('a1')
    expect(mockDeleteApi).toHaveBeenCalledWith('a1')
    expect(res.isSuccess).toBe(true)
  })

  it('delete error', async () => {
    mockDeleteApi.mockResolvedValue(errorResult('Cannot delete default address'))
    const res = await AddressApi.delete('a1')
    expect(res.isSuccess).toBe(false)
    expect(res.message).toBe('Cannot delete default address')
  })
})
