import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockPatch, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockPatch: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  patch: mockPatch,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import type { TaxonRequest } from '../../types/taxon'
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
      'api/catalog/taxons',
      expect.objectContaining({ filter: 'taxonomyId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('TaxonApi.getTaxon', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.getTaxon('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxons/abc-123')
  })
})

describe('TaxonApi.getList', () => {
  it('calls getPaged with list URL and taxonomyId', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await TaxonApi.getList('abc-123', { page: 1, pageSize: 10 })
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxons/list?taxonomyId=abc-123',
      expect.objectContaining({ pageNumber: 1, pageSize: 10 }),
      expect.any(Object),
    )
  })
})

describe('TaxonApi.createTaxon', () => {
  it('calls POST with request body', async () => {
    const req: TaxonRequest = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 0, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'All',
      sortOrder: 'Manual', hideFromNav: false,
    }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonApi.createTaxon(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxons', req)
  })
})

describe('TaxonApi.updateTaxon', () => {
  it('calls PUT with request body', async () => {
    const req: TaxonRequest = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 1, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'Any',
      sortOrder: 'BestSelling', hideFromNav: true,
    }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.updateTaxon('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxons/abc-123', req)
  })
})

describe('TaxonApi.deleteTaxon', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.deleteTaxon('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxons/abc-123')
  })
})

describe('TaxonApi.restoreTaxon', () => {
  it('calls PATCH with restore URL and no body', async () => {
    mockPatch.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.restoreTaxon('abc-123')
    expect(mockPatch).toHaveBeenCalledWith('api/catalog/taxons/abc-123/restore')
  })
})

describe('TaxonApi.repositionTaxon', () => {
  it('calls POST with reposition URL and request body', async () => {
    const req: TaxonRequest = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 2, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'All',
      sortOrder: 'Manual', hideFromNav: false,
    }
    mockPost.mockResolvedValue({ value: { id: '1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.repositionTaxon('abc-123', req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxons/abc-123/reposition', req)
  })
})
