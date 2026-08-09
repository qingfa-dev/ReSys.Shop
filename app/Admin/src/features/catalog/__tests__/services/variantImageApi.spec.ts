import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetBlob, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetBlob: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
  getBlob: mockGetBlob,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { VariantImageApi } from '../../services/variantImageApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantImageApi.listImages', () => {
  it('calls getPaged with variant images URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 100, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantImageApi.listImages('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/catalog/variant-images?variantId=abc-123',
      {},
    )
  })
})

describe('VariantImageApi.getImage', () => {
  it('calls GET with image URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'img-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantImageApi.getImage('img-1')
    expect(mockGet).toHaveBeenCalledWith('api/admin/catalog/variant-images/img-1')
  })
})

describe('VariantImageApi.uploadImage', () => {
  it('calls POST with form data', async () => {
    const file = new File(['x'], 'a.png', { type: 'image/png' })
    mockPost.mockResolvedValue({ value: { id: '1' }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await VariantImageApi.uploadImage({ variantId: 'abc-123', file })
    expect(mockPost).toHaveBeenCalledWith('api/admin/catalog/variant-images', expect.any(FormData))
  })
})

describe('VariantImageApi.updateImage', () => {
  it('calls PUT with image URL and request body', async () => {
    mockPut.mockResolvedValue({ value: { id: 'img-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantImageApi.updateImage('img-1', { alt: 'updated', position: 2, type: 'gallery' })
    expect(mockPut).toHaveBeenCalledWith(
      'api/admin/catalog/variant-images/img-1',
      { alt: 'updated', position: 2, type: 'gallery' },
    )
  })
})

describe('VariantImageApi.deleteImage', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { message: 'ok' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantImageApi.deleteImage('img-1')
    expect(mockDel).toHaveBeenCalledWith('api/admin/catalog/variant-images/img-1')
  })
})

describe('VariantImageApi.downloadImage', () => {
  it('calls getBlob with download URL and returns a Blob', async () => {
    const blob = new Blob(['data'], { type: 'application/octet-stream' })
    mockGetBlob.mockResolvedValue(blob)
    const result = await VariantImageApi.downloadImage('img-1')
    expect(mockGetBlob).toHaveBeenCalledWith('api/admin/catalog/variant-images/img-1/download')
    expect(result).toBe(blob)
  })
})
