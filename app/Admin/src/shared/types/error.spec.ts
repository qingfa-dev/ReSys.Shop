import { describe, it, expect } from 'vitest'
import { ErrorType } from './error'

describe('ErrorType', () => {
  it('maps error types to HTTP status codes', () => {
    expect(ErrorType.BadRequest).toBe(400)
    expect(ErrorType.Unauthorized).toBe(401)
    expect(ErrorType.Forbidden).toBe(403)
    expect(ErrorType.NotFound).toBe(404)
    expect(ErrorType.Conflict).toBe(409)
    expect(ErrorType.Validation).toBe(422)
    expect(ErrorType.Unexpected).toBe(500)
  })
})
