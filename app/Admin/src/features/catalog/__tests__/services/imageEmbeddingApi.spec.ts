import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockPut, mockGet, mockDel } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  put: mockPut,
  get: mockGet,
  del: mockDel,
}))

import { ImageEmbeddingApi } from '../../services/imageEmbeddingApi'

beforeEach(() => {
  vi.clearAllMocks()
})

const embeddingResult = {
  value: {
    id: 'e-1',
    variantImageId: 'img-1',
    modelName: 'openclip-vit-b-32',
    modelVersion: 'v1',
    vector: [],
    dimensions: 0,
    createdAtUtc: '2026-01-01T00:00:00Z',
  },
  isSuccess: true,
  statusCode: 201,
  message: null,
  errors: [],
  metadata: null,
}

describe('ImageEmbeddingApi.create', () => {
  it('calls POST with request body', async () => {
    const req = { variantImageId: 'img-1', modelName: 'openclip-vit-b-32' }
    mockPost.mockResolvedValue(embeddingResult)
    await ImageEmbeddingApi.create(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/catalog/variant-image-embeddings', req)
  })
})

describe('ImageEmbeddingApi.regenerate', () => {
  it('calls PUT with regenerate URL and request body', async () => {
    const req = { variantImageId: 'img-1', modelName: 'openclip-vit-b-32', modelVersion: 'v1' }
    mockPut.mockResolvedValue(embeddingResult)
    await ImageEmbeddingApi.regenerate(req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/catalog/variant-image-embeddings/regenerate', req)
  })
})

describe('ImageEmbeddingApi.get', () => {
  it('calls GET with variantImageId path', async () => {
    const result = { value: embeddingResult.value, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null }
    mockGet.mockResolvedValue(result)
    await ImageEmbeddingApi.get('img-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/catalog/variant-image-embeddings/img-1')
  })
})

describe('ImageEmbeddingApi.deleteEmbedding', () => {
  it('calls DELETE with variantImageId path', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: 'Deleted', errors: [], metadata: null })
    await ImageEmbeddingApi.deleteEmbedding('img-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/catalog/variant-image-embeddings/img-1')
  })
})
