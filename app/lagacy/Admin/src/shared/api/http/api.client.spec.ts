import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from './api.client'
import { parseApiError } from '../utils/api.utils'
import type { AxiosResponse } from 'axios'

vi.mock('../utils/api.utils', () => ({
  parseApiError: vi.fn((err) => ({
    statusCode: err.response?.status || 500,
    title: 'Mock Error',
    message: 'Mock Error',
    detail: 'Mock Detail',
    isSuccess: false,
    errors: {},
    error_code: undefined,
  })),
}))

describe('apiClient', () => {
  let successInterceptor: (response: AxiosResponse) => unknown
  let errorInterceptor: (error: unknown) => Promise<unknown>

  beforeEach(() => {
    vi.clearAllMocks()

    const responseInterceptor = (apiClient.interceptors.response as any).handlers[0]

    if (!responseInterceptor) {
      throw new Error('Response interceptor not found')
    }

    successInterceptor = responseInterceptor.fulfilled
    errorInterceptor = responseInterceptor.rejected
  })

  it('should unwrap successful Result<T> response via value', () => {
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

    const result = successInterceptor(mockResponse) as any

    expect(result).toEqual({
      data: { id: 1, name: 'Test' },
      success: true,
    })
  })

  it('should unwrap successful PagedResult<T> response via items', () => {
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

    const result = successInterceptor(mockResponse) as any

    expect(result).toEqual({
      data: [{ id: 1, name: 'Item 1' }],
      meta: {
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      },
      success: true,
    })
  })

  it('should parse and format error response', async () => {
    const mockError = {
      isAxiosError: true,
      response: {
        status: 400,
        data: { title: 'Bad Request' },
      },
    }

    const result = await errorInterceptor(mockError) as any

    expect(parseApiError).toHaveBeenCalledWith(mockError)
    expect(result).toEqual({
      data: null,
      success: false,
      error: {
        statusCode: 400,
        title: 'Mock Error',
        message: 'Mock Error',
        detail: 'Mock Detail',
        isSuccess: false,
        errors: {},
        error_code: undefined,
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
      error_code: undefined,
    })

    const mockError = {
      response: { status: 401 },
      config: { headers: {} },
    }
    await errorInterceptor(mockError)

    expect(consoleSpy).toHaveBeenCalledWith(expect.stringContaining('Session expired. Attempting to refresh token...'))
    consoleSpy.mockRestore()
  })
})
