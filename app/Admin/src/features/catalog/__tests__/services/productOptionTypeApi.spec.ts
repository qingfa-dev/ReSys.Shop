import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockPut, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  put: mockPut,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { ProductOptionTypeApi } from '../../services/productOptionTypeApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ProductOptionTypeApi.getOptionTypes', () => {
  it('calls getPaged with productId query param', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await ProductOptionTypeApi.getOptionTypes('abc-123')

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/catalog/product-option-types?productId=abc-123',
      {},
    )
  })
})

describe('ProductOptionTypeApi.syncOptionTypes', () => {
  it('calls PUT with sync URL and request body', async () => {
    const req = { productId: 'prod-1', items: [{ optionTypeId: 'ot-1', position: 0 }] }
    mockPut.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductOptionTypeApi.syncOptionTypes(req)
    expect(mockPut).toHaveBeenCalledWith('api/admin/catalog/product-option-types/sync', req)
  })
})

describe('ProductOptionTypeApi.assignOptionTypes', () => {
  it('calls POST with assign URL and request body', async () => {
    const req = { productId: 'prod-1', items: [{ optionTypeId: 'ot-1', position: 0 }] }
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductOptionTypeApi.assignOptionTypes(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/product-option-types/assign', req)
  })
})

describe('ProductOptionTypeApi.revokeOptionTypes', () => {
  it('calls POST with revoke URL and request body', async () => {
    const req = { productId: 'prod-1', items: [{ optionTypeId: 'ot-1', position: 0 }] }
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductOptionTypeApi.revokeOptionTypes(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/product-option-types/revoke', req)
  })
})
