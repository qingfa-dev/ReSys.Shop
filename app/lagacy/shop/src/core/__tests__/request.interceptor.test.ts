import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { InternalAxiosRequestConfig } from 'axios'
import { requestInterceptor } from '../http/interceptors/request.interceptor'

const mockLocalStorage = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}

Object.defineProperty(window, 'localStorage', {
  value: mockLocalStorage,
})

describe('Request Interceptor', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const createMockConfig = (overrides?: Partial<InternalAxiosRequestConfig>): InternalAxiosRequestConfig => {
    return {
      headers: {
        common: {},
        ...overrides?.headers,
      },
      ...overrides,
    } as InternalAxiosRequestConfig
  }

  describe('requestInterceptor', () => {
    it('should add Authorization header when token exists', () => {
      const token = 'test-access-token-12345'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const config = createMockConfig()
      const result = requestInterceptor(config)
      
      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('accessToken')
      expect(result.headers?.Authorization).toBe(`Bearer ${token}`)
    })

    it('should not add Authorization header when no token exists', () => {
      mockLocalStorage.getItem.mockReturnValue(null)
      
      const config = createMockConfig()
      const result = requestInterceptor(config)
      
      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('accessToken')
      expect(result.headers?.Authorization).toBeUndefined()
    })

    it('should return config unmodified when no token', () => {
      mockLocalStorage.getItem.mockReturnValue(null)
      
      const config = createMockConfig({
        url: '/api/products',
        method: 'get',
      })
      
      const result = requestInterceptor(config)
      
      expect(result.url).toBe('/api/products')
      expect(result.method).toBe('get')
    })

    it('should preserve existing headers when adding token', () => {
      const token = 'test-token'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const config = createMockConfig({
        headers: new Headers({ 'Content-Type': 'application/json' }) as unknown as InternalAxiosRequestConfig['headers'],
      })
      
      const result = requestInterceptor(config)
      
      expect(result.headers?.Authorization).toBe(`Bearer ${token}`)
    })

    it('should handle empty string token', () => {
      mockLocalStorage.getItem.mockReturnValue('')
      
      const config = createMockConfig()
      const result = requestInterceptor(config)
      
      // Empty string is falsy, so should not set header
      expect(result.headers?.Authorization).toBeUndefined()
    })

    it('should handle config without headers object', () => {
      const token = 'test-token'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const config = {} as InternalAxiosRequestConfig
      const result = requestInterceptor(config)
      
      // Should handle missing headers gracefully
      expect(result).toBeDefined()
    })

    it('should use correct TOKEN_KEY', () => {
      const token = 'test-token'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const config = createMockConfig()
      requestInterceptor(config)
      
      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('accessToken')
    })

    it('should handle different HTTP methods', () => {
      const token = 'test-token'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const getConfig = createMockConfig({ method: 'get' })
      const postConfig = createMockConfig({ method: 'post' })
      const putConfig = createMockConfig({ method: 'put' })
      const deleteConfig = createMockConfig({ method: 'delete' })
      
      expect(requestInterceptor(getConfig).headers?.Authorization).toBe(`Bearer ${token}`)
      expect(requestInterceptor(postConfig).headers?.Authorization).toBe(`Bearer ${token}`)
      expect(requestInterceptor(putConfig).headers?.Authorization).toBe(`Bearer ${token}`)
      expect(requestInterceptor(deleteConfig).headers?.Authorization).toBe(`Bearer ${token}`)
    })

    it('should handle config with custom baseURL', () => {
      const token = 'test-token'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const config = createMockConfig({
        baseURL: 'https://api.example.com',
      })
      
      const result = requestInterceptor(config)
      
      expect(result.baseURL).toBe('https://api.example.com')
      expect(result.headers?.Authorization).toBe(`Bearer ${token}`)
    })

    it('should pass through all config properties', () => {
      const token = 'test-token'
      mockLocalStorage.getItem.mockReturnValue(token)
      
      const config = createMockConfig({
        params: { page: 1, limit: 10 },
        timeout: 5000,
        withCredentials: true,
      })
      
      const result = requestInterceptor(config)
      
      expect(result.params).toEqual({ page: 1, limit: 10 })
      expect(result.timeout).toBe(5000)
      expect(result.withCredentials).toBe(true)
    })
  })
})