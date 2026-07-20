import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'

const mockParseApiError = vi.fn()
const mockRefreshTokens = vi.fn()
const mockApiClient = vi.fn()
const mockTokenService = {
  getAccessToken: vi.fn(),
  getRefreshToken: vi.fn(),
  setTokens: vi.fn(),
  clearTokens: vi.fn(),
  hasTokens: vi.fn(),
}

vi.mock('@/common/auth/token.service', () => ({
  tokenService: mockTokenService,
}))
vi.mock('../../handlers/error.normalizer', () => ({
  parseApiError: mockParseApiError,
}))
vi.mock('../../handlers/refresh.handler', () => ({
  refreshTokens: mockRefreshTokens,
}))
vi.mock('@/common/api', () => ({
  apiClient: mockApiClient,
}))

describe('errorInterceptor', () => {
  let errorInterceptor: (error: AxiosError) => Promise<any>

  beforeEach(async () => {
    vi.clearAllMocks()
    vi.spyOn(console, 'warn').mockImplementation(() => {})
    const mod = await import('../error.interceptor')
    errorInterceptor = mod.errorInterceptor
  })

  function makeAxiosError(overrides?: { url?: string; headers?: any; _retry?: boolean }): AxiosError {
    const url = overrides?.url ?? '/some/url'
    const headers = overrides && 'headers' in overrides ? overrides.headers : {}
    const _retry = overrides?._retry
    return {
      isAxiosError: true,
      config: { url, headers, _retry } as InternalAxiosRequestConfig & { _retry?: boolean },
      response: undefined as any,
      name: 'AxiosError',
      message: '',
    } as any
  }

  it('returns error result for non-401 errors (404)', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 404,
      title: 'Not Found',
      detail: 'Missing',
      errorCode: 'NOT_FOUND',
      isSuccess: false,
    })

    const result = await errorInterceptor(makeAxiosError())

    expect(result.data.statusCode).toBe(404)
    expect(result.data.isSuccess).toBe(false)
  })

  it('returns unauthorized result for 401 on refresh endpoint without calling refreshTokens', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 401,
      title: 'Unauthorized',
      detail: 'Invalid token',
      errorCode: undefined,
      isSuccess: false,
    })

    const result = await errorInterceptor(makeAxiosError({ url: '/api/store/identity/auth/session/refresh' }))

    expect(result.data.statusCode).toBe(401)
    expect(result.data.errors[0].code).toBe('UNAUTHORIZED')
    expect(mockRefreshTokens).not.toHaveBeenCalled()
  })

  it('refreshes token and retries on 401 for non-refresh endpoint', async () => {
    const mockRetryResponse = { data: 'retry-success', status: 200 }
    mockParseApiError.mockReturnValue({
      statusCode: 401,
      title: 'Unauthorized',
      detail: 'Expired',
      errorCode: undefined,
      isSuccess: false,
    })
    mockRefreshTokens.mockResolvedValue(true)
    mockTokenService.getAccessToken.mockReturnValue('new-token')
    mockApiClient.mockResolvedValue(mockRetryResponse)

    const error = makeAxiosError({ url: '/catalog/products' })
    const result = await errorInterceptor(error)

    expect(mockRefreshTokens).toHaveBeenCalled()
    expect((error.config as any).headers.Authorization).toBe('Bearer new-token')
    expect(mockApiClient).toHaveBeenCalledWith(error.config)
    expect(result).toBe(mockRetryResponse)
  })

  it('returns error result when refresh fails', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 401,
      title: 'Unauthorized',
      detail: 'Expired',
      errorCode: undefined,
      isSuccess: false,
    })
    mockRefreshTokens.mockResolvedValue(false)

    const result = await errorInterceptor(makeAxiosError({ url: '/catalog/products' }))

    expect(result.data.statusCode).toBe(401)
    expect(mockApiClient).not.toHaveBeenCalled()
  })

  it('returns error result when request was already retried', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 401,
      title: 'Unauthorized',
      detail: 'Expired',
      errorCode: undefined,
      isSuccess: false,
    })

    const result = await errorInterceptor(makeAxiosError({ url: '/catalog/products', _retry: true }))

    expect(result.data.statusCode).toBe(401)
    expect(mockRefreshTokens).not.toHaveBeenCalled()
  })

  it('returns error result for 500 server error', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 500,
      title: 'Server Error',
      detail: null,
      errorCode: undefined,
      isSuccess: false,
    })

    const result = await errorInterceptor(makeAxiosError())

    expect(result.data.statusCode).toBe(500)
  })

  it('includes error code in errors array for 422 validation error', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 422,
      title: 'Validation Error',
      detail: 'Invalid input',
      errorCode: 'VALIDATION',
      isSuccess: false,
    })

    const result = await errorInterceptor(makeAxiosError())

    expect(result.data.errors[0].code).toBe('VALIDATION')
  })

  it('does not crash when retrying with null headers', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 401,
      title: 'Unauthorized',
      detail: 'Expired',
      errorCode: undefined,
      isSuccess: false,
    })
    mockRefreshTokens.mockResolvedValue(true)
    mockTokenService.getAccessToken.mockReturnValue('tok')
    mockApiClient.mockResolvedValue({ data: 'ok' })

    const error = makeAxiosError({ url: '/catalog/products', headers: null })
    const result = await errorInterceptor(error)

    expect(mockApiClient).toHaveBeenCalledWith(error.config)
    expect(result).toEqual({ data: 'ok' })
  })

  it('does not set Authorization and still retries when no new token', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 401,
      title: 'Unauthorized',
      detail: 'Expired',
      errorCode: undefined,
      isSuccess: false,
    })
    mockRefreshTokens.mockResolvedValue(true)
    mockTokenService.getAccessToken.mockReturnValue(null)
    mockApiClient.mockResolvedValue({ data: 'ok' })

    const error = makeAxiosError({ url: '/catalog/products' })
    await errorInterceptor(error)

    expect((error.config as any).headers.Authorization).toBeUndefined()
    expect(mockApiClient).toHaveBeenCalledWith(error.config)
  })

  it('uses "Request failed" as fallback message when detail and title are null', async () => {
    mockParseApiError.mockReturnValue({
      statusCode: 500,
      title: null,
      detail: null,
      errorCode: undefined,
      isSuccess: false,
    })

    const result = await errorInterceptor(makeAxiosError())

    expect(result.data.errors[0].message).toBe('Request failed')
  })
})
