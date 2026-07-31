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

import { VariantImageApi } from '../../services/variantImageApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantImageApi.listImages', () => {
  it('calls getPaged with images URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 100, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantImageApi.listImages('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants/abc-123/images',
      expect.objectContaining({ pageNumber: 1, pageSize: 100 }),
    )
  })
})

describe('VariantImageApi.uploadImage', () => {
  it('calls POST with form data', async () => {
    const file = new File(['x'], 'a.png', { type: 'image/png' })
    const formData = new FormData()
    formData.append('file', file)
    mockPost.mockResolvedValue({ value: { id: '1' }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await VariantImageApi.uploadImage('abc-123', file)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/variants/abc-123/images', expect.any(FormData))
  })
})

describe('VariantImageApi.deleteImage', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { message: 'ok' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantImageApi.deleteImage('img-1')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variants/images/img-1')
  })
})
