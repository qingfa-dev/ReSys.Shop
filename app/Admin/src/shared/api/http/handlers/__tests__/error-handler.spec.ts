import { describe, it, expect } from 'vitest'
import { parseApiError, type ParsedApiError } from '../error-handler'

describe('parseApiError', () => {
  it('handles null input', () => {
    const result = parseApiError(null as unknown)
    expect(result).toEqual({
      statusCode: 500,
      title: 'Connection Error',
      message: null,
      detail: 'An unexpected error occurred.',
      isSuccess: false,
      errors: {},
      errorCode: undefined,
    })
  })

  it('handles undefined input', () => {
    const result = parseApiError(undefined as unknown)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
  })

  it('handles non-object input (string)', () => {
    const result = parseApiError('Something went wrong')
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
    expect(result.detail).toBe('An unexpected error occurred.')
  })

  it('handles non-object input (number)', () => {
    const result = parseApiError(42 as unknown)
    expect(result.statusCode).toBe(500)
  })

  it('extracts status from response when apiError body lacks statusCode', () => {
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

  it('extracts status from response when data is missing entirely', () => {
    const error = {
      response: {
        status: 500,
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBeNull()
  })

  it('handles Axios timeouts (ECONNABORTED) as network error', () => {
    const timeoutError = {
      code: 'ECONNABORTED',
      message: 'timeout of 1000ms exceeded',
      request: {},
    }
    const result = parseApiError(timeoutError)
    expect(result.title).toBe('Connection Error')
    expect(result.detail).toBe('timeout of 1000ms exceeded')
  })

  it('handles empty validation errors object', () => {
    const error = {
      response: {
        status: 400,
        data: { title: 'Bad Request', errors: {} },
      },
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({})
  })

  it('prioritizes apiError.statusCode over axios response status', () => {
    const error = {
      response: {
        status: 400,
        data: { statusCode: 422, title: 'Unprocessable' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(422)
  })

  it('extracts camelCase fields from response data', () => {
    const error = {
      response: {
        status: 201,
        data: { statusCode: 201, title: 'Created', detail: 'Resource created', message: 'OK' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(201)
    expect(result.title).toBe('Created')
    expect(result.detail).toBe('Resource created')
    expect(result.message).toBe('OK')
  })

  it('handles PascalCase properties from backend (Status, Title, Detail, ErrorCode)', () => {
    const error = {
      response: {
        data: { Status: 409, Title: 'Conflict', Detail: 'Already exists', ErrorCode: 'Duplicate' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(409)
    expect(result.title).toBe('Conflict')
    expect(result.detail).toBe('Already exists')
    expect(result.errorCode).toBe('Duplicate')
  })

  it('handles PascalCase Message field', () => {
    const error = {
      response: {
        data: { statusCode: 400, Message: 'Validation failed', Title: 'Bad Request' },
      },
    }
    const result = parseApiError(error)
    expect(result.message).toBe('Validation failed')
    expect(result.title).toBe('Bad Request')
  })

  it('handles PascalCase Errors array', () => {
    const error = {
      response: {
        data: { Errors: [{ code: 'Required', message: 'Name is required', type: 0, metadata: null }] },
      },
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({ Required: ['Name is required'] })
  })

  it('handles snake_case error_code', () => {
    const error = {
      response: {
        data: { error_code: 'some_code' },
      },
    }
    const result = parseApiError(error)
    expect(result.errorCode).toBe('some_code')
  })

  it('handles camelCase errorCode in response data', () => {
    const error = {
      response: {
        data: { errorCode: 'camel_code' },
      },
    }
    const result = parseApiError(error)
    expect(result.errorCode).toBe('camel_code')
  })

  it('is idempotent with already parsed errors', () => {
    const parsedError = {
      statusCode: 400,
      title: 'Already Parsed',
      detail: 'Details here',
      errorCode: 'CODE',
      message: 'Some message',
      isSuccess: false,
      errors: {},
    }
    const result = parseApiError(parsedError)
    expect(result.statusCode).toBe(400)
    expect(result.title).toBe('Already Parsed')
    expect(result.detail).toBe('Details here')
    expect(result.errorCode).toBe('CODE')
  })

  it('converts ServerError[] array to Record<string, string[]>', () => {
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

  it('converts ServerError[] with null code to "general" key', () => {
    const error = {
      response: {
        status: 400,
        data: {
          errors: [
            { code: null, message: 'Something went wrong', type: 0, metadata: null },
          ],
        },
      },
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({ general: ['Something went wrong'] })
  })

  it('handles network error (no response, only request)', () => {
    const error = {
      isAxiosError: true,
      request: { _header: 'GET /api/foo' },
      message: 'Network Error',
      response: undefined,
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
    expect(result.detail).toBe('Network Error')
  })

  it('handles network error with missing message', () => {
    const error = {
      isAxiosError: true,
      request: {},
      message: undefined,
      response: undefined,
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Connection Error')
    expect(result.detail).toBe('Network Error. Please check your internet connection.')
  })

  it('handles isAxiosError flag without response data', () => {
    const error = {
      isAxiosError: true,
      response: { status: 500, data: undefined },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(500)
  })

  it('handles 400 with minimal body', () => {
    const error = {
      response: {
        status: 400,
        data: { title: 'Bad Request' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(400)
    expect(result.title).toBe('Bad Request')
    expect(result.detail).toBeNull()
  })

  it('handles 500 with server error title fallback', () => {
    const error = {
      response: {
        status: 500,
        data: { statusCode: 500 },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Server Error')
  })

  it('handles 400 with request error title fallback', () => {
    const error = {
      response: {
        status: 400,
        data: { statusCode: 400 },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(400)
    expect(result.title).toBe('Request Error')
  })

  it('handles non-standard error object with status field', () => {
    const error = { status: 418, title: "I'm a Teapot", message: 'Cannot brew coffee' }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(418)
    expect(result.title).toBe("I'm a Teapot")
    expect(result.message).toBe('Cannot brew coffee')
  })

  it('handles non-standard error object with statusCode field', () => {
    const error = { statusCode: 503, title: 'Service Unavailable', detail: 'Maintenance' }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(503)
    expect(result.title).toBe('Service Unavailable')
    expect(result.detail).toBe('Maintenance')
  })

  it('prioritizes statusCode over status in non-standard objects', () => {
    const error = { statusCode: 200, status: 400, title: 'Mismatch' }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(200)
  })

  it('handles non-standard error with errors as ServerError[]', () => {
    const error = {
      statusCode: 422,
      errors: [
        { code: 'Invalid', message: 'Invalid field', type: 0, metadata: null },
      ],
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(422)
    expect(result.errors).toEqual({ Invalid: ['Invalid field'] })
  })

  it('handles non-standard error with PascalCase Errors', () => {
    const error = {
      statusCode: 400,
      Errors: [{ code: 'Bad', message: 'Bad value', type: 0, metadata: null }],
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({ Bad: ['Bad value'] })
  })

  it('returns errors as-is when not a ServerError[] array', () => {
    const error = {
      response: {
        status: 400,
        data: { errors: ['not', 'an', 'object'] },
      },
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({})
  })

  it('returns empty record for ServerError[] with empty array', () => {
    const error = {
      response: {
        status: 400,
        data: { errors: [] },
      },
    }
    const result = parseApiError(error)
    expect(result.errors).toEqual({})
  })

  it('handles error with data but no recognizable error fields', () => {
    const error = {
      response: {
        status: 500,
        data: { unexpected: 'shape' },
      },
    }
    const result = parseApiError(error)
    expect(result.statusCode).toBe(500)
    expect(result.title).toBe('Server Error')
    expect(result.detail).toBeNull()
  })

  it('returns fallback for completely unrecognizable object', () => {
    const result = parseApiError({ foo: 'bar' })
    expect(result.statusCode).toBe(500)
    expect(result.title).toBeNull()
    expect(result.detail).toBeNull()
  })

  it('sets isSuccess from response data', () => {
    const error = {
      response: {
        status: 200,
        data: { isSuccess: true, value: { id: 1 } },
      },
    }
    const result = parseApiError(error)
    expect(result.isSuccess).toBe(true)
  })

  it('defaults isSuccess to false when missing', () => {
    const error = {
      response: {
        status: 400,
        data: { title: 'Bad' },
      },
    }
    const result = parseApiError(error)
    expect(result.isSuccess).toBe(false)
  })

  it('prefers title over message for detail when detail is absent', () => {
    const error = {
      response: {
        status: 400,
        data: { title: 'Validation Error', message: 'Validation failed' },
      },
    }
    const result = parseApiError(error)
    expect(result.title).toBe('Validation Error')
    expect(result.message).toBe('Validation failed')
  })

  it('uses message as fallback for title when title absent', () => {
    const error = {
      response: {
        status: 400,
        data: { message: 'Something broke' },
      },
    }
    const result = parseApiError(error)
    expect(result.title).toBe('Something broke')
    expect(result.message).toBe('Something broke')
  })

  it('uses message as fallback for detail when detail absent', () => {
    const error = {
      response: {
        status: 400,
        data: { message: 'Something broke' },
      },
    }
    const result = parseApiError(error)
    expect(result.detail).toBe('Something broke')
  })
})
