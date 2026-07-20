import { describe, it, expect, vi, beforeEach } from 'vitest'
import { errorWrapperInterceptor } from '../error-wrapper.interceptor'
import type { AxiosError } from 'axios'

vi.mock('../../../utils/api.utils', () => ({
  parseApiError: vi.fn((err: any) => ({
    statusCode: err.response?.status || 500,
    title: 'Mock Error',
    message: 'Mock Error',
    detail: 'Mock Detail',
    isSuccess: false,
    errors: {},
    errorCode: undefined,
  })),
}))

vi.mock('../../handlers/refresh-handler', () => ({
  refreshTokens: vi.fn(),
}))

import { refreshTokens } from '../../handlers/refresh-handler'

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const asData = (r: any) => r.data as any

describe('errorWrapperInterceptor', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('wraps 400 error into ServerResult', async () => {
    const error = {
      isAxiosError: true,
      response: { status: 400, data: { title: 'Bad Request' } },
      config: { headers: {} },
    } as unknown as AxiosError

    const d = asData(await errorWrapperInterceptor(error))
    expect(d.isSuccess).toBe(false)
    expect(d.statusCode).toBe(400)
    expect(d.errors[0].message).toBe('Mock Detail')
  })

  it('does not attempt refresh on non-401 errors', async () => {
    const error = {
      isAxiosError: true,
      response: { status: 403 },
      config: { headers: {} },
    } as unknown as AxiosError

    await errorWrapperInterceptor(error)
    expect(refreshTokens).not.toHaveBeenCalled()
  })

  it('attempts refresh on 401', async () => {
    vi.mocked(refreshTokens).mockResolvedValue(false)
    const error = {
      response: { status: 401 },
      config: { headers: {} },
    } as unknown as AxiosError

    await errorWrapperInterceptor(error)
    expect(refreshTokens).toHaveBeenCalled()
  })

  it('short-circuits on refresh endpoint 401', async () => {
    const error = {
      response: { status: 401 },
      config: { headers: {}, url: '/auth/session/refresh' },
    } as unknown as AxiosError

    const d = asData(await errorWrapperInterceptor(error))
    expect(refreshTokens).not.toHaveBeenCalled()
    expect(d.statusCode).toBe(401)
    expect(d.errors[0].code).toBe('UNAUTHORIZED')
  })

  it('retries original request after successful refresh', async () => {
    localStorage.setItem('accessToken', 'new-token')
    vi.mocked(refreshTokens).mockResolvedValue(true)

    const error = {
      response: { status: 401 },
      config: { headers: {} },
    } as unknown as AxiosError

    await errorWrapperInterceptor(error)
    expect(refreshTokens).toHaveBeenCalled()
  })

  it('handles network error (no response)', async () => {
    const parseApiError = (await import('../../../utils/api.utils')).parseApiError
    vi.mocked(parseApiError).mockReturnValueOnce({
      statusCode: 500,
      title: 'Connection Error',
      message: null,
      detail: 'Network Error. Please check your internet connection.',
      isSuccess: false,
      errors: {},
      errorCode: undefined,
    })

    const error = {
      isAxiosError: true,
      request: {},
      message: 'Network Error',
      config: { headers: {} },
    } as unknown as AxiosError

    const d = asData(await errorWrapperInterceptor(error))
    expect(d.statusCode).toBe(500)
    expect(d.errors[0].message).toContain('Network Error')
  })
})
