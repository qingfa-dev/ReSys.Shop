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

import { VariantApi } from '../../services/variantApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantApi.getVariants', () => {
  it('calls getPaged with product URL and query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await VariantApi.getVariants('prod-1', { search: 'M', page: 2, pageSize: 25 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants?productId=prod-1',
      expect.objectContaining({ search: 'M', pageNumber: 2, pageSize: 25 }),
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('VariantApi.getVariant', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', sku: 'M' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.getVariant('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/variants/abc-123')
  })
})

describe('VariantApi.createVariant', () => {
  it('calls POST with request body', async () => {
    const req = { sku: 'SHIRT-M', position: 0, trackInventory: true, isMaster: false, productId: 'prod-1', optionValueIds: [] } as any
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await VariantApi.createVariant(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/variants', req)
  })
})

describe('VariantApi.updateVariant', () => {
  it('calls PUT with request body', async () => {
    const req = { sku: 'SHIRT-M', position: 1, trackInventory: true, isMaster: false } as any
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.updateVariant('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/variants/abc-123', req)
  })
})

describe('VariantApi.deleteVariant', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.deleteVariant('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variants/abc-123')
  })
})

describe('VariantApi.getOptionValues', () => {
  it('calls getPaged with option-values URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 0, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantApi.getOptionValues('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants/abc-123/option-values',
      expect.objectContaining({ pageNumber: 1, pageSize: 100 }),
    )
  })
})
