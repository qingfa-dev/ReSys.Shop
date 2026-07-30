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

import { TaxonomyApi } from '../../services/taxonomyApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('TaxonomyApi.getTaxonomies', () => {
  it('calls getPaged with query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await TaxonomyApi.getTaxonomies({ name: 'Categories', page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxonomies',
      { filter: 'name*=Categories', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('TaxonomyApi.getTaxonomy', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Categories' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonomyApi.getTaxonomy('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxonomies/abc-123')
  })
})

describe('TaxonomyApi.createTaxonomy', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Categories', presentation: 'Categories', position: 1 }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonomyApi.createTaxonomy(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxonomies', req)
  })
})

describe('TaxonomyApi.updateTaxonomy', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Categories', presentation: 'Categories', position: 2 }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonomyApi.updateTaxonomy('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxonomies/abc-123', req)
  })
})

describe('TaxonomyApi.deleteTaxonomy', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Categories' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonomyApi.deleteTaxonomy('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxonomies/abc-123')
  })
})
