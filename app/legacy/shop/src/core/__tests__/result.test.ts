import { describe, it, expect } from 'vitest'
import {
  resultHelpers,
  isResultSuccess,
  isResultFailure,
  type Result,
  type PagedResult,
} from '../models/result'

describe('Result type', () => {
  it('should have correct structure for success result', () => {
    const result: Result<string> = {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: 'test data',
    }

    expect(result.isSuccess).toBe(true)
    expect(result.isFailure).toBe(false)
    expect(result.statusCode).toBe(200)
    expect(result.data).toBe('test data')
  })

  it('should have correct structure for failure result', () => {
    const result: Result<string> = {
      isSuccess: false,
      isFailure: true,
      statusCode: 404,
      message: 'Not found',
    }

    expect(result.isSuccess).toBe(false)
    expect(result.isFailure).toBe(true)
    expect(result.statusCode).toBe(404)
    expect(result.message).toBe('Not found')
  })

  it('should support errors array', () => {
    const result: Result<string> = {
      isSuccess: false,
      isFailure: true,
      statusCode: 400,
      message: 'Validation failed',
      errors: [
        { code: 'INVALID_EMAIL', description: 'Email is invalid', field: 'email' },
        { code: 'REQUIRED_FIELD', description: 'Name is required', field: 'name' },
      ],
    }

    expect(result.errors).toHaveLength(2)
    expect(result.errors?.[0]?.field).toBe('email')
    expect(result.errors?.[1]?.code).toBe('REQUIRED_FIELD')
  })
})

describe('PagedResult type', () => {
  it('should have correct pagination structure', () => {
    const result: PagedResult<string> = {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items: ['item1', 'item2', 'item3'],
      page: 1,
      pageSize: 10,
      totalCount: 50,
      totalPages: 5,
      hasNextPage: true,
      hasPreviousPage: false,
    }

    expect(result.items).toHaveLength(3)
    expect(result.page).toBe(1)
    expect(result.pageSize).toBe(10)
    expect(result.totalCount).toBe(50)
    expect(result.totalPages).toBe(5)
    expect(result.hasNextPage).toBe(true)
    expect(result.hasPreviousPage).toBe(false)
  })

  it('should handle last page scenario', () => {
    const result: PagedResult<string> = {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items: ['item1'],
      page: 5,
      pageSize: 10,
      totalCount: 50,
      totalPages: 5,
      hasNextPage: false,
      hasPreviousPage: true,
    }

    expect(result.hasNextPage).toBe(false)
    expect(result.hasPreviousPage).toBe(true)
  })
})

describe('resultHelpers', () => {
  describe('success', () => {
    it('should create success result with default status code', () => {
      const result = resultHelpers.success('test data')

      expect(result.isSuccess).toBe(true)
      expect(result.isFailure).toBe(false)
      expect(result.statusCode).toBe(200)
      expect(result.data).toBe('test data')
    })

    it('should create success result with custom status code', () => {
      const result = resultHelpers.success('test data', 201)

      expect(result.statusCode).toBe(201)
      expect(result.data).toBe('test data')
    })

    it('should handle complex data types', () => {
      const data = { id: 1, name: 'Test', items: ['a', 'b'] }
      const result = resultHelpers.success(data)

      expect(result.data).toEqual(data)
    })

    it('should handle array data', () => {
      const data = ['item1', 'item2', 'item3']
      const result = resultHelpers.success(data)

      expect(result.data).toEqual(data)
    })

    it('should handle null data', () => {
      const result = resultHelpers.success(null)

      expect(result.data).toBe(null)
    })

    it('should handle undefined data', () => {
      const result = resultHelpers.success(undefined)

      expect(result.data).toBe(undefined)
    })
  })

  describe('failure', () => {
    it('should create failure result with default status code', () => {
      const result = resultHelpers.failure('Error message')

      expect(result.isSuccess).toBe(false)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(400)
      expect(result.message).toBe('Error message')
    })

    it('should create failure result with custom status code', () => {
      const result = resultHelpers.failure('Not found', 404)

      expect(result.statusCode).toBe(404)
      expect(result.message).toBe('Not found')
    })

    it('should create failure result with errors array', () => {
      const errors: Result<string>['errors'] = [
        { code: 'ERR001', description: 'Error 1' },
      ]
      const result = resultHelpers.failure('Validation failed', 400, errors)

      expect(result.errors).toEqual(errors)
    })

    it('should handle empty message', () => {
      const result = resultHelpers.failure('')

      expect(result.message).toBe('')
      expect(result.isFailure).toBe(true)
    })

    it('should handle different error status codes', () => {
      const notFound = resultHelpers.failure('Not found', 404)
      const unauthorized = resultHelpers.failure('Unauthorized', 401)
      const serverError = resultHelpers.failure('Server error', 500)

      expect(notFound.statusCode).toBe(404)
      expect(unauthorized.statusCode).toBe(401)
      expect(serverError.statusCode).toBe(500)
    })
  })
})

describe('isResultSuccess', () => {
  it('should return true for success result', () => {
    const result: Result<string> = {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: 'test',
    }

    expect(isResultSuccess(result)).toBe(true)
  })

  it('should return false for failure result', () => {
    const result: Result<string> = {
      isSuccess: false,
      isFailure: true,
      statusCode: 400,
      message: 'Error',
    }

    expect(isResultSuccess(result)).toBe(false)
  })

  it('should return false for ambiguous result (both false)', () => {
    const result: Result<string> = {
      isSuccess: false,
      isFailure: false,
      statusCode: 0,
    }

    expect(isResultSuccess(result)).toBe(false)
  })

  it('should work with different data types', () => {
    const stringResult = resultHelpers.success('test')
    const objectResult = resultHelpers.success({ key: 'value' })
    const arrayResult = resultHelpers.success([1, 2, 3])

    expect(isResultSuccess(stringResult)).toBe(true)
    expect(isResultSuccess(objectResult)).toBe(true)
    expect(isResultSuccess(arrayResult)).toBe(true)
  })
})

describe('isResultFailure', () => {
  it('should return true for failure result', () => {
    const result: Result<string> = {
      isSuccess: false,
      isFailure: true,
      statusCode: 400,
      message: 'Error',
    }

    expect(isResultFailure(result)).toBe(true)
  })

  it('should return false for success result', () => {
    const result: Result<string> = {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: 'test',
    }

    expect(isResultFailure(result)).toBe(false)
  })

  it('should return false for ambiguous result (both false)', () => {
    const result: Result<string> = {
      isSuccess: false,
      isFailure: false,
      statusCode: 0,
    }

    expect(isResultFailure(result)).toBe(false)
  })

  it('should work with different data types', () => {
    const stringResult = resultHelpers.failure('error')
    const objectResult = resultHelpers.failure('error', 400, [{ code: 'ERR', description: 'Error' }])

    expect(isResultFailure(stringResult)).toBe(true)
    expect(isResultFailure(objectResult)).toBe(true)
  })
})