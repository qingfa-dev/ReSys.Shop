import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useOptionTypeStore } from '../option-type.store'
import { OptionTypeApi } from '../../api'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  OptionTypeApi: {
    getMany: mockGetMany,
  },
}))

function pagedResult(overrides: { items?: any[], totalCount?: number } = {}) {
  return { isSuccess: true, statusCode: 200, items: overrides.items ?? [], page: 1, pageSize: 20, totalCount: overrides.totalCount ?? 0, errors: [], message: null, metadata: null }
}

describe('useOptionTypeStore', () => {
  beforeEach(() => { setActivePinia(createPinia()); vi.clearAllMocks() })

  it('has initial state', () => {
    const store = useOptionTypeStore()
    expect(store.loading).toBe(false); expect(store.error).toBeNull()
    expect(store.items).toEqual([]); expect(store.totalRecords).toBe(0)
  })

  it('fetchMany success', async () => {
    mockGetMany.mockResolvedValue(pagedResult({ items: [{ id: '1', name: 'Test' }], totalCount: 1 }))
    const store = useOptionTypeStore(); await store.fetchMany()
    expect(store.loading).toBe(false); expect(store.items).toHaveLength(1)
    expect(store.totalRecords).toBe(1); expect(store.error).toBeNull()
  })
})
