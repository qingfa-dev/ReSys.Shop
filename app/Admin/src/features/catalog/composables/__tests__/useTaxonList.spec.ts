import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'
import { useTaxonList } from '../useTaxonList'
import type { PagedResult } from '@/shared/types/result'

const { mockGetList, mockGetTaxons } = vi.hoisted(() => ({
  mockGetList: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetTaxons: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getList: mockGetList,
    getTaxons: mockGetTaxons,
  },
}))

function okResult(overrides: Partial<PagedResult<{ id: string; name: string }>> = {}): PagedResult<{ id: string; name: string }> {
  return {
    isSuccess: true,
    statusCode: 200,
    items: [{ id: '1', name: 'Test' }],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
    errors: [],
    message: null,
    metadata: null,
    ...overrides,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useTaxonList', () => {
  it('fetches the taxonomy-scoped list when a taxonomyId ref is set', async () => {
    mockGetList.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>('tax1')
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()

    expect(mockGetList).toHaveBeenCalledWith('tax1', expect.objectContaining({ pageNumber: 1 }))
  })

  it('fetches the unscoped list when taxonomyId ref is null', async () => {
    mockGetTaxons.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>(null)
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()

    expect(mockGetTaxons).toHaveBeenCalledWith(expect.objectContaining({ pageNumber: 1 }))
  })

  it('switches to the scoped list when taxonomyId ref changes to a new value', async () => {
    mockGetTaxons.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>(null)
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()
    taxonomyId.value = 'tax2'
    mockGetList.mockResolvedValue(okResult())
    await fetch()

    expect(mockGetList).toHaveBeenLastCalledWith('tax2', expect.objectContaining({ pageNumber: 1 }))
  })

  it('switches back to the unscoped list when taxonomyId ref is cleared', async () => {
    mockGetList.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>('tax1')
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()
    taxonomyId.value = null
    mockGetTaxons.mockResolvedValue(okResult())
    await fetch()

    expect(mockGetTaxons).toHaveBeenLastCalledWith(expect.objectContaining({ pageNumber: 1 }))
  })
})
