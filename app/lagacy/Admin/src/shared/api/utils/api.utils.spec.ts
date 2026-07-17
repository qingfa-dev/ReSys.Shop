import { describe, it, expect } from 'vitest'
import { parseApiError } from './api.utils'


describe('parseApiError - Edge Cases', () => {
  it('should handle null or undefined input gracefully', () => {
    expect(parseApiError(null as unknown)).toEqual({
      statusCode: 500,
      title: 'Connection Error',
      message: null,
      detail: 'An unexpected error occurred.',
      isSuccess: false,
      errors: {},
      error_code: undefined,
    })
  })

  it('should handle non-object inputs', () => {
    expect(parseApiError('Something went wrong')).toEqual({
      statusCode: 500,
      title: 'Connection Error',
      message: null,
      detail: 'An unexpected error occurred.',
      isSuccess: false,
      errors: {},
      error_code: undefined,
    })
  })

  it('should extract status from response if missing in apiError body', () => {
    const error = {
      response: {
        status: 403,
        data: { title: 'Forbidden' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(403)
    expect(result.title).toBe('Forbidden')
  })

  it('should handle Axios timeouts (ECONNABORTED)', () => {
    const timeoutError = {
      code: 'ECONNABORTED',
      message: 'timeout of 1000ms exceeded',
      request: {},
    }
    const result = parseApiError(timeoutError)
    expect(result.title).toBe('Connection Error')
    expect(result.detail).toBe('timeout of 1000ms exceeded')
  })

  it('should handle empty validation errors object', () => {
    const error = {
      response: {
        status: 400,
        data: { title: 'Bad Request', errors: {} },
      },
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({})
  })

  it('should prioritize apiError.statusCode over axios response status', () => {
    const error = {
      response: {
        status: 400,
        data: { statusCode: 422, title: 'Unprocessable' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(422)
  })

  it('should handle PascalCase properties from backend', () => {
    const error = {
      response: {
        data: { Status: 409, Title: 'Conflict', Detail: 'Already exists', ErrorCode: 'Duplicate' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(409)
    expect(result.title).toBe('Conflict')
    expect(result.detail).toBe('Already exists')
    expect(result.error_code).toBe('Duplicate')
  })

  it('should handle snake_case error_code', () => {
    const error = {
      response: {
        data: { error_code: 'some_code' },
      },
    }
    const result = parseApiError(error)
    expect(result.error_code).toBe('some_code')
  })

  it('should be idempotent and return already parsed errors', () => {
    const parsedError = {
      statusCode: 400,
      title: 'Already Parsed',
      detail: 'Details here',
      error_code: 'CODE',
    }
    const result = parseApiError(parsedError)
    expect(result.statusCode).toBe(400)
    expect(result.title).toBe('Already Parsed')
    expect(result.detail).toBe('Details here')
    expect(result.error_code).toBe('CODE')
  })

  it('should convert ServerError[] to Record<string, string[]>', () => {
    const error = {
      response: {
        status: 400,
        data: {
          isSuccess: false,
          statusCode: 400,
          message: 'Validation failed',
          errors: [
            { code: 'Required', message: 'Name is required', type: 0, metadata: null },
            { code: 'Required', message: 'Email is required', type: 0, metadata: null },
            { code: 'InvalidFormat', message: 'Invalid email format', type: 0, metadata: null },
          ],
        },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(400)
    expect(result.isSuccess).toBe(false)
    expect(result.message).toBe('Validation failed')
    expect(result.errors).toEqual({
      Required: ['Name is required', 'Email is required'],
      InvalidFormat: ['Invalid email format'],
    })
  })
})
