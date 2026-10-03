import { describe, it, expect, afterEach } from 'vitest'
import { createApiClient, getApiClient, resetApiClient } from '../axios'

describe('api client factory', () => {
  afterEach(() => {
    resetApiClient()
  })

  it('createApiClient returns an axios instance', () => {
    const client = createApiClient()
    expect(client).toBeDefined()
    expect(typeof client.get).toBe('function')
    expect(typeof client.post).toBe('function')
    expect(typeof client.put).toBe('function')
    expect(typeof client.delete).toBe('function')
    expect(client.defaults.baseURL).toBeDefined()
    expect(client.defaults.timeout).toBe(30000)
  })

  it('getApiClient returns singleton instance', () => {
    const first = getApiClient()
    const second = getApiClient()
    expect(first).toBe(second)
  })

  it('resetApiClient clears the singleton', () => {
    const first = getApiClient()
    resetApiClient()
    const second = getApiClient()
    expect(first).not.toBe(second)
  })

  it('createApiClient returns existing instance on second call', () => {
    const first = createApiClient()
    const second = createApiClient()
    expect(first).toBe(second)
  })
})
