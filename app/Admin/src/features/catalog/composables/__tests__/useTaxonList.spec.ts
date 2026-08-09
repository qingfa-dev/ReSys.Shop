import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'
import { useTaxonList } from '../useTaxonList'
import type { PagedResult } from '@/shared/types/result'

const { mockGetPaged } = vi.hoisted(() => ({ mockGetPaged: vi.fn<(...args: unknown[]) => unknown>() }))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
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
  it('fetches the taxonomy-scoped URL when a taxonomyId ref is set', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>('tax1')
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/catalog/taxons/list?taxonomyId=tax1',
      expect.objectContaining({ pageNumber: 1 }),
      expect.any(Object),
    )
  })

  it('fetches the unscoped URL when taxonomyId ref is null', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>(null)
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/catalog/taxons',
      expect.objectContaining({ pageNumber: 1 }),
      expect.any(Object),
    )
  })

  it('switches to the scoped URL when taxonomyId ref changes to a new value', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>(null)
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()
    taxonomyId.value = 'tax2'
    await fetch()

    expect(mockGetPaged).toHaveBeenLastCalledWith(
      'api/admin/catalog/taxons/list?taxonomyId=tax2',
      expect.objectContaining({ pageNumber: 1 }),
      expect.any(Object),
    )
  })

  it('switches back to the unscoped URL when taxonomyId ref is cleared', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const taxonomyId = ref<string | null>('tax1')
    const { fetch } = useTaxonList(taxonomyId, { immediate: false })

    await fetch()
    taxonomyId.value = null
    await fetch()

    expect(mockGetPaged).toHaveBeenLastCalledWith(
      'api/admin/catalog/taxons',
      expect.objectContaining({ pageNumber: 1 }),
      expect.any(Object),
    )
  })
})
