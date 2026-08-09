import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockDelWithBody, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockDelWithBody: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  delWithBody: mockDelWithBody,
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
      'api/admin/catalog/variant-prices?variantId=abc-123',
      expect.objectContaining({ pageNumber: 1, pageSize: 100 }),
    )
  })
})

describe('VariantPriceApi.setPrice', () => {
  it('calls POST with request body', async () => {
    const req = { variantId: 'abc-123', amount: 10, currency: 'USD' }
    mockPost.mockResolvedValue({ value: { variantId: 'abc-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.setPrice(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/variant-prices', req)
  })
})

describe('VariantPriceApi.removePrice', () => {
  it('calls DELETE with body', async () => {
    mockDelWithBody.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.removePrice('abc-123', 'price-1')
    expect(mockDelWithBody).toHaveBeenCalledWith('api/admin/catalog/variant-prices/price-1', { variantId: 'abc-123', priceId: 'price-1' })
  })
})

describe('VariantPriceApi.syncPrices', () => {
  it('calls POST with sync URL and request body', async () => {
    const req = { variantId: 'abc-123', prices: [{ amount: 10, currency: 'USD' }] }
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.syncPrices(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/variant-prices/sync', req)
  })
})
