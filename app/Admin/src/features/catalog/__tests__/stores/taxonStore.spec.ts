import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetTaxons, mockGetList } = vi.hoisted(() => ({
  mockGetTaxons: vi.fn<any>(),
  mockGetList: vi.fn<any>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getTaxons: mockGetTaxons,
    getList: mockGetList,
  },
}))

import { useTaxonStore } from '../../stores/taxonStore'

function pagedResult(items: any[] = []) {
  return {
    isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 0,
  }
}

describe('useTaxonStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchList calls getTaxons when no taxonomy selected', async () => {
    mockGetTaxons.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    await store.fetchList()
    expect(mockGetTaxons).toHaveBeenCalledWith(expect.objectContaining({ pageSize: 20, searchFields: ['name', 'slug'], searchMode: 'any' }))
    expect(mockGetList).not.toHaveBeenCalled()
  })

  it('fetchList calls getList when a taxonomy is selected', async () => {
    mockGetList.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    store.selectedTaxonomyId = 'tax-1'
    await store.fetchList()
    expect(mockGetList).toHaveBeenCalledWith('tax-1', expect.objectContaining({ pageSize: 20 }))
    expect(mockGetTaxons).not.toHaveBeenCalled()
  })

  it('setSearch updates search and refetches', async () => {
    mockGetTaxons.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    await store.setSearch('shoes')
    expect(store.search).toBe('shoes')
    expect(store.page).toBe(1)
    expect(mockGetTaxons).toHaveBeenCalledWith(expect.objectContaining({ search: 'shoes' }))
  })

  it('setSelectedTaxonomy switches endpoint and refetches', async () => {
    mockGetTaxons.mockResolvedValue(pagedResult([]))
    mockGetList.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    await store.setSelectedTaxonomy('tax-1')
    expect(store.selectedTaxonomyId).toBe('tax-1')
    expect(mockGetList).toHaveBeenCalled()
    await store.setSelectedTaxonomy(null)
    expect(store.selectedTaxonomyId).toBeNull()
    expect(mockGetTaxons).toHaveBeenCalled()
  })

  it('fetchActive is lazy-once and populates activeTaxons', async () => {
    const items = [{ id: '1', name: 'Shoes' }]
    mockGetTaxons.mockResolvedValue(pagedResult(items))
    const store = useTaxonStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetTaxons).toHaveBeenCalledTimes(1)
    expect(store.activeTaxons).toEqual(items)
    expect(store.loaded).toBe(true)
  })
})
