import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { VariantPriceApi } from '../../services/variantPriceApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantPriceApi.listPrices', () => {
  it('calls getPaged with prices URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 100, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantPriceApi.listPrices('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants/abc-123/prices',
      expect.objectContaining({ pageNumber: 1, pageSize: 100 }),
    )
  })
})

describe('VariantPriceApi.setPrice', () => {
  it('calls POST with request body', async () => {
    const req = { amount: 10, currency: 'USD' }
    mockPost.mockResolvedValue({ value: { variantId: 'abc-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.setPrice('abc-123', req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/variants/abc-123/prices', req)
  })
})

describe('VariantPriceApi.removePrice', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.removePrice('abc-123', 'price-1')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variants/abc-123/prices/price-1')
  })
})
