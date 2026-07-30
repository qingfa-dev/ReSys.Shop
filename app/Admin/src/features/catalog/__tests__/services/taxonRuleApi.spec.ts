import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { TaxonRuleApi } from '../../services/taxonRuleApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('TaxonRuleApi.getRules', () => {
  it('calls getPaged with taxonId in URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 9999, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await TaxonRuleApi.getRules('taxon-123')

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxonomies/taxons/taxon-123/rules',
      expect.objectContaining({ filter: 'taxonId=taxon-123' }),
    )
  })
})

describe('TaxonRuleApi.createRule', () => {
  it('calls POST with correct URL and body', async () => {
    const req = { type: 'product_name', matchPolicy: 'contains', value: 'Nike' }
    mockPost.mockResolvedValue({ value: { id: '1', taxonId: 'taxon-123', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonRuleApi.createRule('taxon-123', req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/taxon-123/rules', req)
  })
})

describe('TaxonRuleApi.updateRule', () => {
  it('calls PUT with correct URL and body', async () => {
    const req = { type: 'product_name', matchPolicy: 'is_equal_to', value: 'Adidas' }
    mockPut.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.updateRule('taxon-123', 'rule-456', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/taxon-123/rules/rule-456', req)
  })
})

describe('TaxonRuleApi.deleteRule', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.deleteRule('taxon-123', 'rule-456')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/taxon-123/rules/rule-456')
  })
})
