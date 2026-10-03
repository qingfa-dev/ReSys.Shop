import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
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

import type { VariantRequest } from '../../types/variant'
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

    await VariantApi.getVariants('prod-1', { search: 'M', pageNumber: 2, pageSize: 25 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/catalog/variants?productId=prod-1',
      expect.objectContaining({ search: 'M', pageNumber: 2, pageSize: 25 }),
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })

  it('omits productId query param when productId is empty', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await VariantApi.getVariants('', { pageSize: 100 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/catalog/variants',
      expect.objectContaining({ pageSize: 100 }),
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('VariantApi.getVariant', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', sku: 'M' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.getVariant('abc-123')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/catalog/variants/abc-123')
  })
})

describe('VariantApi.createVariant', () => {
  it('calls POST with request body', async () => {
    const req: VariantRequest = { sku: 'SHIRT-M', position: 0, trackInventory: true, isMaster: false, productId: 'prod-1', optionValueIds: [] }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await VariantApi.createVariant(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/catalog/variants', req)
  })
})

describe('VariantApi.updateVariant', () => {
  it('calls PUT with request body', async () => {
    const req: VariantRequest = { sku: 'SHIRT-M', position: 1, trackInventory: true, isMaster: false, productId: 'prod-1' }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.updateVariant('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/catalog/variants/abc-123', req)
  })
})

describe('VariantApi.deleteVariant', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.deleteVariant('abc-123')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/catalog/variants/abc-123')
  })
})

describe('VariantApi.getOptionValues', () => {
  it('calls getPaged with option-values URL and no paging params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 0, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantApi.getOptionValues('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/catalog/variant-option-values?variantId=abc-123',
      {},
    )
  })
})

describe('VariantApi.assignOptionValues', () => {
  it('calls POST with assign URL and body', async () => {
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.assignOptionValues('abc-123', ['ov-1', 'ov-2'])
    expect(mockPost).toHaveBeenCalledWith(
      '/api/admin/catalog/variant-option-values/assign',
      { variantId: 'abc-123', optionValueIds: ['ov-1', 'ov-2'] },
    )
  })
})

describe('VariantApi.revokeOptionValues', () => {
  it('calls POST with revoke URL and body', async () => {
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.revokeOptionValues('abc-123', ['ov-1', 'ov-2'])
    expect(mockPost).toHaveBeenCalledWith(
      '/api/admin/catalog/variant-option-values/revoke',
      { variantId: 'abc-123', optionValueIds: ['ov-1', 'ov-2'] },
    )
  })
})

describe('VariantApi.syncOptionValues', () => {
  it('calls PUT with sync URL and body', async () => {
    mockPut.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.syncOptionValues('abc-123', ['ov-1', 'ov-2'])
    expect(mockPut).toHaveBeenCalledWith(
      '/api/admin/catalog/variant-option-values/sync',
      { variantId: 'abc-123', optionValueIds: ['ov-1', 'ov-2'] },
    )
  })
})
