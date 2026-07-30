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

import { TaxonApi } from '../../services/taxonApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('TaxonApi.getTaxons', () => {
  it('calls getPaged with query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await TaxonApi.getTaxons({ taxonomyId: 'abc-123' })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxonomies/taxons',
      expect.objectContaining({ filter: 'taxonomyId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('TaxonApi.getTaxon', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.getTaxon('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/abc-123')
  })
})

describe('TaxonApi.getTree', () => {
  it('calls GET with tree URL', async () => {
    mockGet.mockResolvedValue({ value: { tree: [] }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.getTree()
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/tree')
  })
})

describe('TaxonApi.createTaxon', () => {
  it('calls POST with request body', async () => {
    const req = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 0, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'All',
      sortOrder: 'Manual', hideFromNav: false,
    } as any
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonApi.createTaxon(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxonomies/taxons', req)
  })
})

describe('TaxonApi.updateTaxon', () => {
  it('calls PUT with request body', async () => {
    const req = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 1, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'Any',
      sortOrder: 'BestSelling', hideFromNav: true,
    } as any
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.updateTaxon('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/abc-123', req)
  })
})

describe('TaxonApi.deleteTaxon', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.deleteTaxon('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/abc-123')
  })
})
