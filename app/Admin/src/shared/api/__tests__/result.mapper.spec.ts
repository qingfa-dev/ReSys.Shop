import { describe, it, expect } from 'vitest'
import { mapToErrors, resultToMapped, isSuccess, isFailure } from '../utils/result.mapper'
import type { Result, Error } from '@/shared/models'

describe('mapToErrors', () => {
  it('should map Error array to Record<string, string[]>', () => {
    const errors: Error[] = [
      { code: 'Name', message: 'Name is required', type: 0, metadata: null },
      { code: 'Name', message: 'Name too short', type: 0, metadata: null },
      { code: 'Email', message: 'Invalid email', type: 0, metadata: null },
    ]
    const result = mapToErrors(errors)
    expect(result).toEqual({
      Name: ['Name is required', 'Name too short'],
      Email: ['Invalid email'],
    })
  })

  it('should use "general" key for errors without code', () => {
    const errors: Error[] = [
      { code: '', message: 'Something went wrong', type: 0, metadata: null },
    ]
    const result = mapToErrors(errors)
    expect(result).toEqual({ general: ['Something went wrong'] })
  })
})

describe('resultToMapped', () => {
  it('should map success result', () => {
    const result: Result<string> = {
      isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: 'test'
    }
    const mapped = resultToMapped(result)
    expect(mapped.success).toBe(true)
    expect(mapped.data).toBe('test')
  })

  it('should map failure result', () => {
    const result: Result<string> = {
      isSuccess: false,
      statusCode: 400,
      errors: [{ code: 'Name', message: 'Required', type: 0, metadata: null }],
      message: 'Validation failed',
      metadata: null,
      value: '' as string,
    }
    const mapped = resultToMapped(result)
    expect(mapped.success).toBe(false)
    expect((mapped as { error: unknown }).error).toBeDefined()
  })
})

describe('isSuccess / isFailure', () => {
  it('should return true for success result', () => {
    const r: Result<null> = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null }
    expect(isSuccess(r)).toBe(true)
    expect(isFailure(r)).toBe(false)
  })

  it('should return false for failure result', () => {
    const r: Result<null> = { isSuccess: false, statusCode: 400, errors: [], message: null, metadata: null, value: null }
    expect(isSuccess(r)).toBe(false)
    expect(isFailure(r)).toBe(true)
  })
})
