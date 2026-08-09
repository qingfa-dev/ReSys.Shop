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

import { ProductClassificationApi } from '../../services/productClassificationApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ProductClassificationApi.getClassifications', () => {
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

    await ProductClassificationApi.getClassifications('abc-123')

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/catalog/product-classifications?productId=abc-123',
      {},
    )
  })
})

describe('ProductClassificationApi.syncClassifications', () => {
  it('calls PUT with sync URL and request body', async () => {
    const req = { productId: 'prod-1', items: [{ taxonId: 'tx-1', position: 0 }] }
    mockPut.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductClassificationApi.syncClassifications(req)
    expect(mockPut).toHaveBeenCalledWith('api/admin/catalog/product-classifications/sync', req)
  })
})

describe('ProductClassificationApi.assignClassifications', () => {
  it('calls POST with assign URL and request body', async () => {
    const req = { productId: 'prod-1', items: [{ taxonId: 'tx-1', position: 0 }] }
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductClassificationApi.assignClassifications(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/product-classifications/assign', req)
  })
})

describe('ProductClassificationApi.revokeClassifications', () => {
  it('calls POST with revoke URL and request body', async () => {
    const req = { productId: 'prod-1', items: [{ taxonId: 'tx-1', position: 0 }] }
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductClassificationApi.revokeClassifications(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/product-classifications/revoke', req)
  })
})
