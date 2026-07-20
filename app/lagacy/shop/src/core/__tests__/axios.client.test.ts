import { describe, it, expect } from 'vitest'
import { httpClient } from '../http'

describe('Axios Client', () => {
  describe('httpClient (with interceptors)', () => {
    it('should be defined', () => {
      expect(httpClient).toBeDefined()
    })

    it('should have HTTP methods', () => {
      expect(typeof httpClient.get).toBe('function')
      expect(typeof httpClient.post).toBe('function')
      expect(typeof httpClient.put).toBe('function')
      expect(typeof httpClient.patch).toBe('function')
      expect(typeof httpClient.delete).toBe('function')
    })

    it('should have default baseURL configured', () => {
      expect(httpClient.defaults.baseURL).toBe('/api')
    })

    it('should have default timeout configured', () => {
      expect(httpClient.defaults.timeout).toBe(30000)
    })

    it('should have Content-Type header set', () => {
      expect(httpClient.defaults.headers['Content-Type']).toBe('application/json')
    })

    it('should have Accept header set', () => {
      expect(httpClient.defaults.headers['Accept']).toBe('application/json')
    })

    it('should have withCredentials set to false', () => {
      expect(httpClient.defaults.withCredentials).toBe(false)
    })

    it('should have interceptors configured', () => {
      expect(httpClient.interceptors).toBeDefined()
      expect(httpClient.interceptors.request).toBeDefined()
      expect(httpClient.interceptors.response).toBeDefined()
    })

    it('should have request interceptors registered', () => {
      expect(httpClient.interceptors.request.handlers?.length ?? 0).toBeGreaterThan(0)
    })

    it('should have response interceptors registered', () => {
      expect(httpClient.interceptors.response.handlers?.length ?? 0).toBeGreaterThan(0)
    })
  })
})