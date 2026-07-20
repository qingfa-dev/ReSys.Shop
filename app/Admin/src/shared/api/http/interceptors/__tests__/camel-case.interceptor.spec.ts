import { describe, it, expect, vi } from 'vitest'
import type { AxiosResponse } from 'axios'

vi.mock('@/shared/utils/object.transforms', () => ({
  toCamelCaseKeys: vi.fn((obj: Record<string, unknown>) => {
    const result: Record<string, unknown> = {}
    for (const key of Object.keys(obj)) {
      const camelKey = key.replace(/_([a-z])/g, (_, c: string) => c.toUpperCase())
      result[camelKey] = obj[key]
    }
    return result
  }),
}))

describe('camelCaseInterceptor', () => {
  it('transforms snake_case response data keys', async () => {
    const { camelCaseInterceptor } = await import('../camel-case.interceptor')

    const response = {
      data: { snake_case: 'value', already: 'val' },
    } as AxiosResponse
    const result = camelCaseInterceptor(response)

    expect(result.data).toEqual({ snakeCase: 'value', already: 'val' })
  })

  it('passes through null data unchanged', async () => {
    const { camelCaseInterceptor } = await import('../camel-case.interceptor')

    const response = { data: null } as AxiosResponse
    const result = camelCaseInterceptor(response)

    expect(result.data).toBeNull()
  })

  it('passes through non-object data unchanged', async () => {
    const { camelCaseInterceptor } = await import('../camel-case.interceptor')

    const response = { data: 'string' } as unknown as AxiosResponse
    const result = camelCaseInterceptor(response)

    expect(result.data).toBe('string')
  })
})
