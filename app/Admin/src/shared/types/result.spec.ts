import { describe, it, expect } from 'vitest'
import {
  StatusCode,
  isSuccess,
  isFailure,
  ok,
  created,
  noContent,
  failure,
  badRequest,
  notFound,
  unauthorized,
  forbidden,
  conflict,
  validation,
  unexpected,
  pagedOk,
  pagedFailure,
} from './result'

describe('StatusCode', () => {
  it('includes common HTTP codes', () => {
    expect(StatusCode.Ok).toBe(200)
    expect(StatusCode.Created).toBe(201)
    expect(StatusCode.NoContent).toBe(204)
    expect(StatusCode.BadRequest).toBe(400)
    expect(StatusCode.NotFound).toBe(404)
    expect(StatusCode.InternalServerError).toBe(500)
  })
})

describe('isSuccess / isFailure', () => {
  it('returns true for success results', () => {
    const result = ok('data')
    expect(isSuccess(result)).toBe(true)
    expect(isFailure(result)).toBe(false)
  })

  it('returns false for failure results', () => {
    const result = badRequest('invalid')
    expect(isSuccess(result)).toBe(false)
    expect(isFailure(result)).toBe(true)
  })
})

describe('ok', () => {
  it('creates a success result with value', () => {
    const result = ok(42)
    expect(result.isSuccess).toBe(true)
    expect(result.statusCode).toBe(200)
    expect(result.value).toBe(42)
    expect(result.errors).toHaveLength(0)
  })
})

describe('created', () => {
  it('creates a 201 success result', () => {
    const result = created({ id: 'abc' })
    expect(result.isSuccess).toBe(true)
    expect(result.statusCode).toBe(201)
    expect(result.value).toEqual({ id: 'abc' })
  })
})

describe('noContent', () => {
  it('creates a 204 success result with null value', () => {
    const result = noContent()
    expect(result.isSuccess).toBe(true)
    expect(result.statusCode).toBe(204)
    expect(result.value).toBeNull()
  })
})

describe('failure', () => {
  it('creates a failure result from an error', () => {
    const err = { code: 'NotFound', message: 'Not found', type: 404 }
    const result = failure<string>(err)
    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(404)
    expect(result.errors).toEqual([err])
    expect(result.value).toBeNull()
  })
})

describe('badRequest', () => {
  it('creates a 400 failure', () => {
    const result = badRequest('Invalid input')
    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(400)
    expect(result.errors[0]!.code).toBe('BadRequest')
  })
})

describe('notFound', () => {
  it('creates a 404 failure', () => {
    const result = notFound('Product not found')
    expect(result.statusCode).toBe(404)
    expect(result.errors[0]!.message).toBe('Product not found')
  })
})

describe('unauthorized', () => {
  it('creates a 401 failure with default message', () => {
    const result = unauthorized()
    expect(result.statusCode).toBe(401)
    expect(result.errors[0]!.message).toBe('Authentication required.')
  })
})

describe('forbidden', () => {
  it('creates a 403 failure', () => {
    const result = forbidden()
    expect(result.statusCode).toBe(403)
    expect(result.errors[0]!.message).toBe('Access denied.')
  })
})

describe('conflict', () => {
  it('creates a 409 failure', () => {
    const result = conflict('Duplicate entry')
    expect(result.statusCode).toBe(409)
    expect(result.errors[0]!.code).toBe('Conflict')
  })
})

describe('validation', () => {
  it('creates a 422 failure with multiple errors', () => {
    const errors = [
      { code: 'Required', message: 'Name is required', type: 422 },
      { code: 'MinLength', message: 'Name too short', type: 422 },
    ]
    const result = validation(errors)
    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(422)
    expect(result.errors).toHaveLength(2)
    expect(result.message).toBe('Validation failed.')
  })
})

describe('unexpected', () => {
  it('creates a 500 failure', () => {
    const result = unexpected()
    expect(result.statusCode).toBe(500)
    expect(result.errors[0]!.code).toBe('Unexpected')
  })
})

describe('pagedOk', () => {
  it('creates a success paged result with computed totalPages', () => {
    const items = [{ id: 'a' }, { id: 'b' }]
    const result = pagedOk(items, 1, 10, 25)
    expect(result.isSuccess).toBe(true)
    expect(result.statusCode).toBe(200)
    expect(result.items).toEqual(items)
    expect(result.page).toBe(1)
    expect(result.pageSize).toBe(10)
    expect(result.totalCount).toBe(25)
    expect(result.totalPages).toBe(3)
  })

  it('computes totalPages as 0 for pageSize <= 0', () => {
    const result = pagedOk([], 1, 0, 10)
    expect(result.totalPages).toBe(0)
  })
})

describe('pagedFailure', () => {
  it('creates a failure paged result with errors', () => {
    const errors = [{ code: 'NotFound', message: 'Not found', type: 404 }]
    const result = pagedFailure(errors, 404)
    expect(result.isSuccess).toBe(false)
    expect(result.statusCode).toBe(404)
    expect(result.errors).toEqual(errors)
    expect(result.items).toEqual([])
    expect(result.page).toBe(1)
    expect(result.totalPages).toBe(0)
  })

  it('defaults to 500 status code', () => {
    const result = pagedFailure([{ code: 'Error', message: 'Oops', type: StatusCode.InternalServerError }])
    expect(result.statusCode).toBe(StatusCode.InternalServerError)
  })

  it('isSuccess and isFailure work with paged results', () => {
    const ok = pagedOk([], 1, 10, 0)
    const fail = pagedFailure([{ code: 'Err', message: 'err', type: 400 }])
    expect(isSuccess(ok)).toBe(true)
    expect(isFailure(fail)).toBe(true)
  })
})
