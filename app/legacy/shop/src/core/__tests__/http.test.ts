import { describe, it, expect } from 'vitest'
import { httpClient } from '../http'

describe('HTTP Module Exports', () => {
  describe('httpClient', () => {
    it('should be defined', () => {
      expect(httpClient).toBeDefined()
    })

    it('should have get method', () => {
      expect(typeof httpClient.get).toBe('function')
    })

    it('should have post method', () => {
      expect(typeof httpClient.post).toBe('function')
    })

    it('should have put method', () => {
      expect(typeof httpClient.put).toBe('function')
    })

    it('should have patch method', () => {
      expect(typeof httpClient.patch).toBe('function')
    })

    it('should have delete method', () => {
      expect(typeof httpClient.delete).toBe('function')
    })

    it('should have interceptors', () => {
      expect(httpClient.interceptors).toBeDefined()
      expect(httpClient.interceptors.request).toBeDefined()
      expect(httpClient.interceptors.response).toBeDefined()
    })

    it('should have defaults configured', () => {
      expect(httpClient.defaults).toBeDefined()
      expect(httpClient.defaults.baseURL).toBe('/api')
      expect(httpClient.defaults.timeout).toBe(30000)
    })

    it('should have request interceptors configured', () => {
      // The request interceptor should be registered
      const requestInterceptors = httpClient.interceptors.request.handlers
      expect(requestInterceptors?.length ?? 0).toBeGreaterThan(0)
    })

    it('should have response interceptors configured', () => {
      // The response interceptor should be registered
      const responseInterceptors = httpClient.interceptors.response.handlers
      expect(responseInterceptors?.length ?? 0).toBeGreaterThan(0)
    })
  })
})