import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetTaxon, mockGetRules } = vi.hoisted(() => ({
  mockGetTaxon: vi.fn<any>(),
  mockGetRules: vi.fn<any>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getTaxon: mockGetTaxon,
  },
}))

vi.mock('../../services/taxonRuleApi', () => ({
  TaxonRuleApi: {
    getRules: mockGetRules,
  },
}))

import { useTaxonDetailStore } from '../../stores/taxonDetailStore'

describe('useTaxonDetailStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchDetail populates currentTaxon on success', async () => {
    const detail = { id: 't1', name: 'Shoes' }
    mockGetTaxon.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null, value: detail })
    const store = useTaxonDetailStore()
    const result = await store.fetchDetail('t1')
    expect(result.isSuccess).toBe(true)
    expect(store.currentTaxon).toEqual(detail)
  })

  it('fetchDetail returns failure and keeps currentTaxon unchanged', async () => {
    mockGetTaxon.mockResolvedValue({ isSuccess: false, statusCode: 404, message: 'Not found', errors: [{ code: 'NotFound', message: 'Not found', type: 404 }], metadata: null, value: null })
    const store = useTaxonDetailStore()
    store.currentTaxon = { id: 'old' } as any
    const result = await store.fetchDetail('t1')
    expect(result.isSuccess).toBe(false)
    expect(store.currentTaxon).toEqual({ id: 'old' })
  })

  it('fetchRules populates rules on success', async () => {
    const items = [{ id: 'r1', type: 'product_name', matchPolicy: 'contains', value: 'shoes', taxonId: 't1' }]
    mockGetRules.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null, items, page: 1, pageSize: 10, totalCount: 1, totalPages: 1 })
    const store = useTaxonDetailStore()
    const result = await store.fetchRules('t1')
    expect(result.isSuccess).toBe(true)
    expect(store.rules).toEqual(items)
  })
})
