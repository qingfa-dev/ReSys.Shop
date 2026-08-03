import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveTaxonomies } from '../useActiveTaxonomies'
import { TaxonomyApi } from '../../services/taxonomyApi'
import type { PagedResult } from '@/shared/types/result'
import type { TaxonomyListItem } from '../../types/taxonomy'

vi.mock('../../services/taxonomyApi', () => ({
  TaxonomyApi: { getTaxonomies: vi.fn<() => Promise<PagedResult<TaxonomyListItem>>>() },
}))

const mockGetTaxonomies = vi.mocked(TaxonomyApi.getTaxonomies)

function okResult(items: TaxonomyListItem[] = [{ id: 't1', name: 'Category', presentation: 'Category', position: 1, taxonsCount: 0, createdAtUtc: '2026-01-01T00:00:00Z', modifiedAtUtc: null }]): PagedResult<TaxonomyListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveTaxonomies', () => {
  it('loads all taxonomies via the TaxonomyApi', async () => {
    mockGetTaxonomies.mockResolvedValue(okResult())
    const { items, load } = useActiveTaxonomies()

    await load()

    expect(mockGetTaxonomies).toHaveBeenCalledWith({})
    expect(items.value).toHaveLength(1)
  })
})
