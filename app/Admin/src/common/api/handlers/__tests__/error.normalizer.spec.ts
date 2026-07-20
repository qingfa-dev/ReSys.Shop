import { describe, it, expect } from 'vitest'
import { parseApiError, normalizeServerErrors } from '../error.normalizer'

describe('normalizeServerErrors', () => {
  it('returns empty object for null/undefined', () => {
    expect(normalizeServerErrors(null)).toEqual({})
    expect(normalizeServerErrors(undefined)).toEqual({})
  })

  it('returns empty object for empty array', () => {
    expect(normalizeServerErrors([])).toEqual({})
  })

  it('converts ServerError array to Record<string, string[]>', () => {
    const input = [
      { code: 'Validation.Field', message: 'Field is required', type: 422, metadata: null },
      { code: 'Validation.Field', message: 'Field too long', type: 422, metadata: null },
      { code: 'Conflict', message: 'Already exists', type: 409, metadata: null },
    ]
    expect(normalizeServerErrors(input)).toEqual({
      'Validation.Field': ['Field is required', 'Field too long'],
      Conflict: ['Already exists'],
    })
  })

  it('falls back to "general" for errors without code', () => {
    expect(normalizeServerErrors([{ code: '', message: 'Oops', type: 500, metadata: null }])).toEqual({
      general: ['Oops'],
    })
  })

  it('returns pre-shaped Record directly', () => {
    expect(normalizeServerErrors({ field: ['bad'] })).toEqual({ field: ['bad'] })
  })
})

describe('parseApiError', () => {
  it('returns 500 Connection Error for null input', () => {
    const result = parseApiError(null)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
    expect(result.isSuccess).toBe(false)
  })

  it('returns 500 for non-object input', () => {
    const result = parseApiError('string')
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
  })

  it('extracts data from Axios response error', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 404,
        data: {
          statusCode: 404,
          title: 'Not Found',
          detail: 'Resource not found',
          isSuccess: false,
          errors: [],
        },
      },
      request: {},
    }
    const result = parseApiError(axiosError)
    expect(result.statusCode).toBe(404)
    expect(result.title).toBe('Not Found')
    expect(result.detail).toBe('Resource not found')
    expect(result.isSuccess).toBe(false)
  })

  it('falls back to response.status when data has no statusCode', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 422,
        data: {
          title: 'Validation Error',
          isSuccess: false,
        },
      },
      request: {},
    }
    const result = parseApiError(axiosError)
    expect(result.statusCode).toBe(422)
    expect(result.title).toBe('Validation Error')
  })

  it('handles network error (no response)', () => {
    const axiosError = {
      isAxiosError: true,
      request: {},
      message: 'Network Error',
    }
    const result = parseApiError(axiosError)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
    expect(result.detail).toBe('Network Error')
  })

  it('handles raw error object with statusCode', () => {
    const rawError = {
      statusCode: 503,
      message: 'Service Unavailable',
      isSuccess: false,
      errors: [],
    }
    const result = parseApiError(rawError)
    expect(result.statusCode).toBe(503)
    expect(result.message).toBe('Service Unavailable')
  })
})
