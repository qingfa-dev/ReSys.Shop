import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockPatch, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockPatch: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
  patch: mockPatch,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { ProductApi } from '../../services/productApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ProductApi.getProducts', () => {
  it('calls getPaged with query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await ProductApi.getProducts({ status: 'Active', page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/products',
      { filter: 'status=Active', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('ProductApi.getProduct', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Shirt' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.getProduct('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/products/abc-123')
  })
})

describe('ProductApi.createProduct', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Shirt', slug: 'shirt', description: null, metaTitle: null, metaDescription: null, metaKeywords: null, availableOn: null, discontinueOn: null, trackInventory: true, styleCode: null, seasonName: null, materialComposition: null, careInstructions: null, fitNotes: null, department: null, genderTarget: null }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await ProductApi.createProduct(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/products', req)
  })
})

describe('ProductApi.updateProduct', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Shirt', slug: 'shirt', description: null, metaTitle: null, metaDescription: null, metaKeywords: null, availableOn: null, discontinueOn: null, trackInventory: false, styleCode: null, seasonName: null, materialComposition: null, careInstructions: null, fitNotes: null, department: null, genderTarget: null }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.updateProduct('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/products/abc-123', req)
  })
})

describe('ProductApi.deleteProduct', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Shirt' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.deleteProduct('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/products/abc-123')
  })
})

describe('ProductApi.activateProduct', () => {
  it('calls PATCH with activate URL', async () => {
    mockPatch.mockResolvedValue({ value: { id: '1', name: 'Shirt' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.activateProduct('abc-123')
    expect(mockPatch).toHaveBeenCalledWith('api/catalog/products/abc-123/activate')
  })
})

describe('ProductApi.discontinueProduct', () => {
  it('calls PATCH with discontinue URL', async () => {
    mockPatch.mockResolvedValue({ value: { id: '1', name: 'Shirt' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.discontinueProduct('abc-123')
    expect(mockPatch).toHaveBeenCalledWith('api/catalog/products/abc-123/discontinue')
  })
})
