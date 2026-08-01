import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetTree } = vi.hoisted(() => ({
  mockGetTree: vi.fn<any>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getTree: mockGetTree,
  },
}))

import { useTaxonTreeStore } from '../../stores/taxonTreeStore'

function treeResult() {
  return {
    items: [{ id: 'n1', name: 'Root', children: [] }],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    metadata: null,
  }
}

describe('useTaxonTreeStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchTree fetches and caches on first call', async () => {
    mockGetTree.mockResolvedValue(treeResult())
    const store = useTaxonTreeStore()
    const result = await store.fetchTree('tax-1')
    expect(result.isSuccess).toBe(true)
    expect(mockGetTree).toHaveBeenCalledWith('tax-1')
    expect(store.tree).toHaveLength(1)
    expect(store.treeTaxonomyId).toBe('tax-1')
  })

  it('fetchTree does not refetch for the same taxonomy', async () => {
    mockGetTree.mockResolvedValue(treeResult())
    const store = useTaxonTreeStore()
    await store.fetchTree('tax-1')
    await store.fetchTree('tax-1')
    expect(mockGetTree).toHaveBeenCalledTimes(1)
  })

  it('fetchTree refetches for a different taxonomy', async () => {
    mockGetTree.mockResolvedValue(treeResult())
    const store = useTaxonTreeStore()
    await store.fetchTree('tax-1')
    await store.fetchTree('tax-2')
    expect(mockGetTree).toHaveBeenCalledTimes(2)
    expect(store.treeTaxonomyId).toBe('tax-2')
  })
})
