import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useStockItemStore } from '../stock-item.store'
import { StockItemApi } from '../../api'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  StockItemApi: {
    getMany: mockGetMany,
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

function errorResult() {
  return { isSuccess: false, statusCode: 400, value: null, errors: [], message: 'Something went wrong', metadata: null }
}

describe('useStockItemStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('has initial state', () => {
    const store = useStockItemStore()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
    expect(store.items).toEqual([])
    expect(store.totalRecords).toBe(0)
    expect(store.query.page).toBe(1)
  })

  it('fetchMany success', async () => {
    mockGetMany.mockResolvedValue(pagedResult({ items: [{ id: '1', sku: 'SKU-1' }], totalCount: 1 }))
    const store = useStockItemStore()
    await store.fetchMany()
    expect(store.loading).toBe(false)
    expect(store.items).toHaveLength(1)
    expect(store.totalRecords).toBe(1)
    expect(store.error).toBeNull()
  })

  it('fetchMany failure', async () => {
    mockGetMany.mockResolvedValue(errorResult())
    const store = useStockItemStore()
    await store.fetchMany()
    expect(store.loading).toBe(false)
    expect(store.error).toBe('Something went wrong')
    expect(store.items).toEqual([])
    expect(store.totalRecords).toBe(0)
  })

  it('fetchMany network error', async () => {
    mockGetMany.mockRejectedValue(new Error('Network'))
    const store = useStockItemStore()
    await store.fetchMany()
    expect(store.loading).toBe(false)
    expect(store.error).toBe('Failed to load')
    expect(store.items).toEqual([])
    expect(store.totalRecords).toBe(0)
  })

  it('setPage updates query and re-fetches', async () => {
    mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
    const store = useStockItemStore()
    await store.setPage(3)
    expect(store.query.page).toBe(3)
    expect(mockGetMany).toHaveBeenCalled()
  })

  it('setSearch sets search and resets page', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useStockItemStore()
    await store.setPage(3)
    await store.setSearch('test')
    expect(mockGetMany).toHaveBeenCalled()
    expect(store.query.search).toEqual({ value: 'test', mode: 'Any' })
    expect(store.query.page).toBe(1)
  })

  it('setSort updates sort clause', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useStockItemStore()
    await store.setSort('sku', 'Ascending')
    expect(store.query.sort).toEqual([{ field: 'sku', direction: 'Ascending' }])
  })

  it('setFilter sets filter group and resets page', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useStockItemStore()
    await store.setFilter({ logic: 'And', conditions: [{ field: 'status', operator: 'Equal', value: 'Active' }], groups: [] })
    expect(mockGetMany).toHaveBeenCalled()
    expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'status', operator: 'Equal', value: 'Active' }], groups: [] })
    expect(store.query.page).toBe(1)
  })

  it('resetQuery restores defaults', async () => {
    mockGetMany.mockResolvedValue(pagedResult())
    const store = useStockItemStore()
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
    const store = useStockItemStore()
    const promise = store.fetchMany()
    expect(store.loading).toBe(true)
    resolver(pagedResult())
    await promise
  })
})
