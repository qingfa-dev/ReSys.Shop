import { describe, it, expect } from 'vitest'
import { ApiError, ErrorCode, isApiError } from '../errors'

describe('ApiError', () => {
  it('captures status and message', () => {
    const e = new ApiError(404, 'not found')
    expect(e.status).toBe(404)
    expect(e.message).toBe('not found')
    expect(e.name).toBe('ApiError')
    expect(isApiError(e)).toBe(true)
  })
})

describe('ErrorCode', () => {
  it('exposes known codes', () => {
    expect(ErrorCode.NotFound).toBe(404)
    expect(ErrorCode.Unauthorized).toBe(401)
    expect(ErrorCode.Forbidden).toBe(403)
    expect(ErrorCode.Validation).toBe(422)
  })
})

describe('isApiError', () => {
  it('returns false for non-ApiError values', () => {
    expect(isApiError(new Error('x'))).toBe(false)
    expect(isApiError('x')).toBe(false)
    expect(isApiError(null)).toBe(false)
  })
})
