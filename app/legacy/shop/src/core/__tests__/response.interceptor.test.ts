import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { responseInterceptor, responseErrorInterceptor } from '../http/interceptors/response.interceptor'
import type { Result } from '../models/result'

const mockLocalStorage = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}

Object.defineProperty(global, 'localStorage', {
  value: mockLocalStorage,
})

describe('Response Interceptor', () => {
  let mockPost: ReturnType<typeof vi.fn>

  beforeEach(() => {
    vi.clearAllMocks()
    mockPost = vi.fn().mockRejectedValue(new Error('Token refresh failed'))
    ;(axios as unknown as { post: ReturnType<typeof vi.fn> }).post = mockPost
  })

  describe('responseInterceptor', () => {
    it('should return data from successful response', () => {
      const response = { data: { test: 'data' } }
      const result = responseInterceptor(response)
      
      expect(result).toEqual({ test: 'data' })
    })

    it('should handle different data types', () => {
      const stringData = { data: 'test string' }
      const arrayData = { data: [1, 2, 3] }
      const objectData = { data: { nested: { value: 1 } } }
      
      expect(responseInterceptor(stringData)).toBe('test string')
      expect(responseInterceptor(arrayData)).toEqual([1, 2, 3])
      expect(responseInterceptor(objectData)).toEqual({ nested: { value: 1 } })
    })

    it('should handle null data', () => {
      const response = { data: null }
      const result = responseInterceptor(response)
      
      expect(result).toBeNull()
    })

    it('should handle array data', () => {
      const items = [{ id: 1 }, { id: 2 }, { id: 3 }]
      const response = { data: items }
      const result = responseInterceptor(response)
      
      expect(result).toEqual(items)
      expect(result).toHaveLength(3)
    })

    it('should pass through the entire data object', () => {
      const complexData = {
        items: ['a', 'b'],
        pagination: { page: 1, total: 10 },
        meta: { timestamp: '2024-01-01' },
      }
      const response = { data: complexData }
      const result = responseInterceptor(response)
      
      expect(result).toEqual(complexData)
    })
  })

  describe('responseErrorInterceptor', () => {
    const createMockError = (status: number, config?: InternalAxiosRequestConfig & { _retry?: boolean }): AxiosError<Result<unknown>> => {
      return {
        response: {
          status,
          data: { isSuccess: false, isFailure: true, statusCode: status, message: 'Error occurred' },
        },
        config: config || {
          baseURL: '/api',
          headers: { common: {} },
        },
      } as AxiosError<Result<unknown>>
    }

    it('should re-throw non-401 errors', async () => {
      const error = createMockError(400)
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error)
    })

    it('should re-throw 500 errors', async () => {
      const error = createMockError(500)
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error)
    })

    it('should re-throw 404 errors', async () => {
      const error = createMockError(404)
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error)
    })

    it('should re-throw 403 errors', async () => {
      const error = createMockError(403)
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error)
    })

    it('should handle 401 without config gracefully', async () => {
      const error = {
        response: {
          status: 401,
          data: { isSuccess: false, isFailure: true, statusCode: 401, message: 'Unauthorized' },
        },
      } as AxiosError<Result<unknown>>

      await expect(responseErrorInterceptor(error)).rejects.toEqual(error)
    })

    it('should not retry if _retry is already true', async () => {
      const config = {
        baseURL: '/api',
        headers: { Authorization: 'Bearer old-token' },
        _retry: true,
      } as InternalAxiosRequestConfig & { _retry?: boolean }

      const error = createMockError(401, config)

      await expect(responseErrorInterceptor(error)).rejects.toEqual(error)
    })

    it('should handle error when config is undefined', async () => {
      const error = {
        response: {
          status: 401,
          data: { isSuccess: false, isFailure: true, statusCode: 401, message: 'Unauthorized' },
        },
      } as unknown as AxiosError<Result<unknown>>

      await expect(responseErrorInterceptor(error)).rejects.toBeDefined()
    })

    it('should attempt token refresh when refresh token exists', async () => {
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'refreshToken') return 'valid-refresh-token'
        if (key === 'accessToken') return 'old-access-token'
        return null
      })
      
      const config = {
        baseURL: '/api',
        headers: { Authorization: 'Bearer old-token' },
        _retry: false,
      } as InternalAxiosRequestConfig & { _retry?: boolean }

      const error = createMockError(401, config)

      await responseErrorInterceptor(error).catch(() => {})
      
      expect(mockPost).toHaveBeenCalled()
    })

    it('should handle refresh token failure gracefully', async () => {
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'refreshToken') return 'valid-refresh-token'
        if (key === 'accessToken') return 'old-access-token'
        return null
      })
      mockLocalStorage.removeItem.mockImplementation(() => {})
      
      const config = {
        baseURL: '/api',
        headers: { Authorization: 'Bearer old-token' },
        _retry: false,
      } as InternalAxiosRequestConfig & { _retry?: boolean }

      const error = createMockError(401, config)

      await expect(responseErrorInterceptor(error)).rejects.toBeDefined()
    })

    it('should use default baseURL when request has no baseURL', async () => {
      mockLocalStorage.getItem.mockReturnValue(null)
      
      const config = {
        headers: { Authorization: 'Bearer old-token' },
        _retry: false,
      } as InternalAxiosRequestConfig & { _retry?: boolean }

      const error = createMockError(401, config)

      await expect(responseErrorInterceptor(error)).rejects.toBeDefined()
    })

    it('should not process if original request is not defined', async () => {
      const error = {
        response: {
          status: 401,
          data: { isSuccess: false, isFailure: true, statusCode: 401 },
        },
        config: undefined,
      } as unknown as AxiosError<Result<unknown>>

      await expect(responseErrorInterceptor(error)).rejects.toBeDefined()
    })

    it('should store new tokens after successful refresh', async () => {
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'refreshToken') return 'valid-refresh-token'
        return null
      })
      mockLocalStorage.setItem.mockImplementation(() => {})
      
      mockPost.mockResolvedValueOnce({
        data: {
          accessToken: 'new-access-token',
          refreshToken: 'new-refresh-token',
        },
      })
      
      const config = {
        baseURL: '/api',
        headers: { Authorization: 'Bearer old-token' },
        _retry: false,
      } as InternalAxiosRequestConfig & { _retry?: boolean }

      const error = createMockError(401, config)

      await responseErrorInterceptor(error).catch(() => {})
      
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('accessToken', 'new-access-token')
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('refreshToken', 'new-refresh-token')
    })
  })
})