import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { errorInterceptor } from '../interceptors/error'

const { mockRequest, mockRefresh } = vi.hoisted(() => ({
  mockRequest: vi.fn(),
  mockRefresh: vi.fn(),
}))

vi.mock('../axios', () => ({
  getApiClient: vi.fn(() => ({ request: mockRequest })),
}))

vi.mock('../interceptors/refresh', () => ({
  handleTokenRefresh: mockRefresh,
}))

function axiosError(overrides: Record<string, unknown> = {}) {
  const err = new Error((overrides.message as string) ?? 'Request failed')
  ;(err as any).isAxiosError = true
  ;(err as any).config = overrides.config ?? { headers: {}, url: '/api/products' }
  ;(err as any).response = overrides.response ?? null
  return err
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('errorInterceptor', () => {
  it('passes through canceled errors', async () => {
    await expect(errorInterceptor(new axios.CanceledError('Canceled'))).rejects.toThrow('Canceled')
  })

  it('wraps non-axios errors as HttpError(0)', async () => {
    const err = new Error('Boom')
    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 0,
      errors: [{ code: 'Unexpected' }],
    })
  })

  it('extracts errors from response body', async () => {
    const err = axiosError({
      response: {
        status: 422,
        data: { errors: [{ code: 'Validation', message: 'Invalid' }] },
      },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 422,
      errors: [{ code: 'Validation', message: 'Invalid' }],
    })
  })

  it('extracts title and code from problem details', async () => {
    const err = axiosError({
      response: {
        status: 400,
        data: { title: 'Bad Request', code: 'BadRequest' },
      },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 400,
      errors: [{ code: 'BadRequest', message: 'Bad Request' }],
    })
  })

  it('retries on 401 and returns response on success', async () => {
    mockRefresh.mockResolvedValue('new-token')
    mockRequest.mockResolvedValue({ data: 'retried' })
    const config = { headers: {} as Record<string, string>, url: '/api/products' }
    const err = axiosError({
      config,
      response: { status: 401, data: null },
    })

    const result = await errorInterceptor(err)
    expect(result).toEqual({ data: 'retried' })
    expect(mockRefresh).toHaveBeenCalledOnce()
    expect(config.headers.Authorization).toBe('Bearer new-token')
    expect(mockRequest).toHaveBeenCalledWith(config)
  })

  it('rejects with HttpError(401) when refresh fails', async () => {
    mockRefresh.mockRejectedValue(new Error('Refresh failed'))
    const config = { headers: {} as Record<string, string>, url: '/api/products' }
    const err = axiosError({
      config,
      response: { status: 401, data: null },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 401,
      errors: [{ code: 'Unauthorized' }],
    })
  })

  it('does not retry if request already retried', async () => {
    const config = { headers: {} as Record<string, string>, url: '/api/products', _retry: true }
    const err = axiosError({
      config,
      response: { status: 401, data: null },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 401,
    })
    expect(mockRefresh).not.toHaveBeenCalled()
  })

  it('does not retry for refresh endpoint', async () => {
    const config = { headers: {} as Record<string, string>, url: '/api/identity/auth/sessions/refresh' }
    const err = axiosError({
      config,
      response: { status: 401, data: null },
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 401,
    })
    expect(mockRefresh).not.toHaveBeenCalled()
  })

  it('handles network error without response', async () => {
    const err = axiosError({
      response: null,
      message: 'Network Error',
    })

    await expect(errorInterceptor(err)).rejects.toMatchObject({
      statusCode: 0,
      errors: [{ code: 'HttpError' }],
    })
  })
})
