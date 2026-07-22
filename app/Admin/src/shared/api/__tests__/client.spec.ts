import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '../client'
import { parseApiError } from '../handlers/error-handler'
import type { AxiosResponse } from 'axios'
import type { Result } from '@/shared/models'

vi.mock('../handlers/error-handler', () => ({
  parseApiError: vi.fn<(err: { response?: { status: number } }) => Record<string, unknown>>((err) => ({
    statusCode: err.response?.status || 500,
    title: 'Mock Error',
    message: 'Mock Error',
    detail: 'Mock Detail',
    isSuccess: false,
    errors: {},
      errorCode: undefined,
  })),
}))

vi.mock('../handlers/refresh-handler', () => ({
  refreshTokens: vi.fn().mockResolvedValue(false),
}))

vi.mock('@/router', () => ({
  default: { push: vi.fn() },
}))

describe('apiClient', () => {
  let successInterceptor: (response: AxiosResponse) => unknown
  let errorInterceptor: (error: unknown) => Promise<unknown>

  beforeEach(() => {
    vi.clearAllMocks()

    const responseInterceptor = (apiClient.interceptors.response as unknown as { handlers: Array<{ fulfilled: (response: AxiosResponse) => unknown; rejected: (error: unknown) => Promise<unknown> }> }).handlers[0]

    if (!responseInterceptor) {
      throw new Error('Response interceptor not found')
    }

    successInterceptor = responseInterceptor.fulfilled
    errorInterceptor = responseInterceptor.rejected
  })

  it('should return raw AxiosResponse from success interceptor', () => {
    const mockResponse = {
      data: {
        isSuccess: true,
        statusCode: 200,
        errors: [],
        message: null,
        metadata: null,
        value: { id: 1, name: 'Test' },
      },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: {},
    } as AxiosResponse

    const result = successInterceptor(mockResponse)

    expect(result).toBe(mockResponse)
  })

  it('should return raw AxiosResponse for paged response from success interceptor', () => {
    const mockResponse = {
      data: {
        isSuccess: true,
        statusCode: 200,
        errors: [],
        message: null,
        metadata: null,
        items: [{ id: 1, name: 'Item 1' }],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: {},
    } as AxiosResponse

    const result = successInterceptor(mockResponse)

    expect(result).toBe(mockResponse)
  })

  it('should wrap error response as Result<null> in AxiosResponse', async () => {
    const mockError = {
      isAxiosError: true,
      response: {
        status: 400,
        data: { title: 'Bad Request' },
      },
    }

    const result = await errorInterceptor(mockError) as unknown as { data: Result<null> }

    expect(parseApiError).toHaveBeenCalledWith(mockError)
    expect(result).toMatchObject({
      data: {
        isSuccess: false,
        statusCode: 400,
        errors: [{ code: 'ERROR', message: 'Mock Detail', type: 0, metadata: null }],
        message: 'Mock Error',
        metadata: null,
        value: null,
      },
    })
  })

  it('should log warning on 401 status', async () => {
    const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(() => {})

    vi.mocked(parseApiError).mockReturnValueOnce({
      statusCode: 401,
      title: null,
      message: null,
      detail: null,
      isSuccess: false,
      errors: {},
    errorCode: undefined,
    })

    const mockError = {
      response: { status: 401 },
      config: { headers: {} },
    }
    await errorInterceptor(mockError).catch(() => {})

    expect(consoleSpy).toHaveBeenCalledWith(expect.stringContaining('Session expired. Attempting to refresh token...'))
    consoleSpy.mockRestore()
  })
})
