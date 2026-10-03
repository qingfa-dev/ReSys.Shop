import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { ref } from 'vue'
import { useEmbeddingStatus } from '../useEmbeddingStatus'
import type { EmbeddingDetailResponse } from '../../types/imageEmbedding'

const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/imageEmbeddingApi', () => ({
  ImageEmbeddingApi: { get: mockGet },
}))

function okEmbedding(overrides: Partial<EmbeddingDetailResponse> = {}): { isSuccess: true; value: EmbeddingDetailResponse } {
  return {
    isSuccess: true,
    value: {
      id: 'e-1', variantImageId: 'img-1', modelName: 'fashion-clip', modelVersion: 'v1',
      vector: [], dimensions: 512,
      status: 'Completed', error: undefined, hangfireJobId: 'job-1', completedAtUtc: '2026-01-01T00:00:00Z',
      createdAtUtc: '2026-01-01T00:00:00Z',
      ...overrides,
    },
  }
}

function notFound(): { isSuccess: false; errors: Array<{ code: string }> } {
  return { isSuccess: false, errors: [{ code: 'ImageEmbedding.NotFound' }] }
}

beforeEach(() => { vi.clearAllMocks(); vi.useFakeTimers() })
afterEach(() => { vi.useRealTimers() })

describe('useEmbeddingStatus', () => {
  it('refresh: sets embedding on success', async () => {
    mockGet.mockResolvedValue(okEmbedding())
    const imageId = ref<string | null>('img-1')
    const { embedding, loading, refresh } = useEmbeddingStatus(imageId)

    await refresh()

    expect(embedding.value).not.toBeNull()
    expect(embedding.value!.status).toBe('Completed')
    expect(loading.value).toBe(false)
  })

  it('refresh: sets null on 404', async () => {
    mockGet.mockResolvedValue(notFound())
    const imageId = ref<string | null>('img-1')
    const { embedding, loading, refresh } = useEmbeddingStatus(imageId)

    await refresh()

    expect(embedding.value).toBeNull()
    expect(loading.value).toBe(false)
  })

  it('refresh: does nothing when variantImageId is null', async () => {
    const imageId = ref<string | null>(null)
    const { refresh } = useEmbeddingStatus(imageId)

    await refresh()

    expect(mockGet).not.toHaveBeenCalled()
  })

  it('poll: stops on Completed', async () => {
    mockGet.mockResolvedValue(okEmbedding({ status: 'Pending' }))
    const imageId = ref<string | null>('img-1')
    const { embedding, poll } = useEmbeddingStatus(imageId)

    const pollPromise = poll()
    await vi.advanceTimersByTimeAsync(0)
    mockGet.mockResolvedValue(okEmbedding({ status: 'Processing' }))
    await vi.advanceTimersByTimeAsync(1500)
    mockGet.mockResolvedValue(okEmbedding({ status: 'Completed' }))
    await vi.advanceTimersByTimeAsync(1500)
    await pollPromise

    expect(embedding.value!.status).toBe('Completed')
    expect(mockGet).toHaveBeenCalledTimes(3)
  })

  it('poll: stops on Failed', async () => {
    mockGet.mockResolvedValue(okEmbedding({ status: 'Pending' }))
    const imageId = ref<string | null>('img-1')
    const { embedding, poll } = useEmbeddingStatus(imageId)

    const pollPromise = poll()
    await vi.advanceTimersByTimeAsync(0)
    mockGet.mockResolvedValue(okEmbedding({ status: 'Failed', error: 'Inference timeout' }))
    await vi.advanceTimersByTimeAsync(1500)
    await pollPromise

    expect(embedding.value!.status).toBe('Failed')
    expect(embedding.value!.error).toBe('Inference timeout')
    expect(mockGet).toHaveBeenCalledTimes(2)
  })

  it('poll: times out after max attempts', async () => {
    mockGet.mockResolvedValue(okEmbedding({ status: 'Pending' }))
    const imageId = ref<string | null>('img-1')
    const { error, poll } = useEmbeddingStatus(imageId)

    const pollPromise = poll(3, 100)
    await vi.advanceTimersByTimeAsync(0)
    await vi.advanceTimersByTimeAsync(100)
    await vi.advanceTimersByTimeAsync(100)
    await vi.advanceTimersByTimeAsync(100)
    await pollPromise

    expect(error.value).toContain('timed out')
    expect(mockGet).toHaveBeenCalledTimes(3)
  })
})
