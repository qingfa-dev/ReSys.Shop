import { describe, it, expect, vi, beforeEach } from 'vitest'
import { camelCaseInterceptor } from '../camelcase.interceptor'
import type { AxiosResponse } from 'axios'

vi.mock('@/common/mapper/mapper.utils', () => ({
  toCamelCaseKeys: vi.fn((data: Record<string, unknown>) => ({
    ...data,
    converted: true,
  })),
}))

describe('camelCaseInterceptor', () => {
  it('converts response data keys', () => {
    const response = {
      data: { id: 1, name: 'Test' },
      status: 200,
    } as AxiosResponse
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual({ id: 1, name: 'Test', converted: true })
  })

  it('handles null data without error', () => {
    const response = { data: null, status: 200 } as AxiosResponse
    const result = camelCaseInterceptor(response)
    expect(result.data).toBeNull()
  })

  it('handles non-object data (string)', () => {
    const response = { data: 'plain string', status: 200 } as AxiosResponse
    const result = camelCaseInterceptor(response)
    expect(result.data).toBe('plain string')
  })

  it('handles empty object', () => {
    const response = { data: {}, status: 200 } as AxiosResponse
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual({ converted: true })
  })

  it('returns the same response object', () => {
    const response = { data: { x: 1 }, status: 200 } as AxiosResponse
    const result = camelCaseInterceptor(response)
    expect(result).toBe(response)
  })
})
