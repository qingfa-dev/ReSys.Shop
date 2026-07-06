import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockRequest } = vi.hoisted(() => ({
  mockRequest: vi.fn(),
}))

vi.mock('axios', () => {
  const onRejectedHandlers: Array<(e: Record<string, unknown>) => never> = []
  const instance = Object.assign(
    vi.fn((config: Record<string, unknown>) => {
      const result = (mockRequest as ReturnType<typeof vi.fn>)(config) as {
        data: unknown
        status: number
      }
      const validate =
        ((config.validateStatus as (s: number) => boolean) ?? ((s: number) => s >= 200 && s < 300)) as (
          s: number,
        ) => boolean
      if (!validate(result.status)) {
        const error = {
          isAxiosError: true,
          response: { status: result.status, data: result.data },
          message: `Request failed with status code ${result.status}`,
        }
        for (const handler of onRejectedHandlers) {
          return Promise.reject(handler(error))
        }
        return Promise.reject(error)
      }
      return Promise.resolve(result)
    }),
    {
      interceptors: {
        request: { use: vi.fn() },
        response: {
          use: vi.fn(
            (_onFulfilled: unknown, onRejected?: (e: Record<string, unknown>) => never) => {
              if (onRejected) onRejectedHandlers.push(onRejected)
            },
          ),
        },
      },
    },
  )
  return {
    default: { create: vi.fn(() => instance), isAxiosError: vi.fn(() => true) },
  }
})

import { api, ApiError } from '../client'

describe('api client', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('returns parsed JSON for 2xx responses', async () => {
    mockRequest.mockReturnValue({ data: { id: 1 }, status: 200 })
    const result = await api.get<{ id: number }>('/x')
    expect(result).toEqual({ id: 1 })
  })

  it('throws ApiError on non-2xx', async () => {
    mockRequest.mockReturnValue({ data: 'nope', status: 404 })
    await expect(api.get('/x')).rejects.toBeInstanceOf(ApiError)
  })

  it('returns undefined for 204 responses', async () => {
    mockRequest.mockReturnValue({ data: null, status: 204 })
    const result = await api.delete('/x')
    expect(result).toBeUndefined()
  })
})
