import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useTaxonDetail } from '../useTaxonDetail'
import { TaxonApi } from '../../services/taxonApi'
import { TaxonRuleApi } from '../../services/taxonRuleApi'
import type { Result, PagedResult } from '@/shared/types'
import type { TaxonDetail } from '../../types/taxon'
import type { TaxonRuleListItem } from '../../types/taxonRule'

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: { getTaxon: vi.fn<() => Promise<Result<TaxonDetail>>>() },
}))
vi.mock('../../services/taxonRuleApi', () => ({
  TaxonRuleApi: { getRules: vi.fn<() => Promise<PagedResult<TaxonRuleListItem>>>() },
}))

const mockGetTaxon = vi.mocked(TaxonApi.getTaxon)
const mockGetRules = vi.mocked(TaxonRuleApi.getRules)

function okDetail(): Result<TaxonDetail> {
  const detail: TaxonDetail = {
    id: 't1', taxonomyId: 'tax1', parentId: null,
    name: 'Shoes', presentation: 'Shoes', description: null, slug: 'shoes', position: 1,
    metaTitle: null, metaDescription: null, metaKeywords: null,
    imageUrl: null, squareImageUrl: null, automatic: false,
    rulesMatchPolicy: 'All', sortOrder: 'Manual', hideFromNav: false,
    parentName: null, taxonomyName: null, lft: 1, rgt: 2, depth: 0,
    childrenCount: 0, taxonRuleCount: 0, productCount: 0,
    permalink: '/shoes', prettyName: 'Shoes',
    createdAtUtc: '2026-01-01T00:00:00Z', modifiedAtUtc: null,
  }
  return { isSuccess: true, statusCode: 200, value: detail, errors: [], message: null, metadata: null }
}

function okRules(): PagedResult<TaxonRuleListItem> {
  return { isSuccess: true, statusCode: 200, items: [{ id: 'r1', taxonId: 't1', type: 'Name', matchPolicy: 'All', value: 'x' }], page: 1, pageSize: 20, totalCount: 1, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useTaxonDetail', () => {
  it('loads the taxon detail and stores it in currentTaxon', async () => {
    mockGetTaxon.mockResolvedValue(okDetail())
    const { currentTaxon, fetchDetail } = useTaxonDetail()

    const result = await fetchDetail('t1')

    expect(mockGetTaxon).toHaveBeenCalledWith('t1')
    expect(result.isSuccess).toBe(true)
    expect(currentTaxon.value?.name).toBe('Shoes')
  })

  it('loads the taxon rules into rules', async () => {
    mockGetRules.mockResolvedValue(okRules())
    const { rules, fetchRules } = useTaxonDetail()

    const result = await fetchRules('t1')

    expect(mockGetRules).toHaveBeenCalledWith('t1')
    expect(result.isSuccess).toBe(true)
    expect(rules.value).toHaveLength(1)
  })
})
