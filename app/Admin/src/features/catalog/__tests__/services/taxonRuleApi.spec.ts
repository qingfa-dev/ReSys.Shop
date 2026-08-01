import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockDelWithBody, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockDelWithBody: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
  delWithBody: mockDelWithBody,
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
      'api/catalog/taxon-rules?taxonId=taxon-123',
      expect.objectContaining({ filter: 'taxonId=taxon-123' }),
    )
  })
})

describe('TaxonRuleApi.createRule', () => {
  it('calls POST with correct URL and body', async () => {
    const req = { taxonId: 'taxon-123', type: 'product_name', matchPolicy: 'contains', value: 'Nike' }
    mockPost.mockResolvedValue({ value: { id: '1', taxonId: 'taxon-123', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonRuleApi.createRule(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxon-rules', req)
  })
})

describe('TaxonRuleApi.updateRule', () => {
  it('calls PUT with correct URL and body', async () => {
    const req = { taxonId: 'taxon-123', type: 'product_name', matchPolicy: 'is_equal_to', value: 'Adidas' }
    mockPut.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.updateRule('rule-456', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxon-rules/rule-456', req)
  })
})

describe('TaxonRuleApi.deleteRule', () => {
  it('calls DELETE with correct URL and body', async () => {
    mockDelWithBody.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.deleteRule('taxon-123', 'rule-456')
    expect(mockDelWithBody).toHaveBeenCalledWith('api/catalog/taxon-rules/rule-456', { taxonId: 'taxon-123', ruleId: 'rule-456' })
  })
})

describe('TaxonRuleApi.syncRules', () => {
  it('calls POST with correct URL and body', async () => {
    const rules = [
      { id: 'rule-456', type: 'product_name', matchPolicy: 'is_equal_to', value: 'Adidas' },
      { type: 'product_sku', matchPolicy: 'contains', value: 'NK' },
    ]
    mockPost.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.syncRules({ taxonId: 'taxon-123', rules })
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxon-rules/sync', { taxonId: 'taxon-123', rules })
  })
})
