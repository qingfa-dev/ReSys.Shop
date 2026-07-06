import { describe, it, expect } from 'vitest'
import type { Envelope } from '../envelope'

describe('Envelope<T>', () => {
  it('matches backend Result<T> shape', () => {
    const ok: Envelope<{ id: string }> = {
      isSuccess: true,
      value: { id: '1' },
      errors: [],
    }
    const fail: Envelope<never> = {
      isSuccess: false,
      value: null,
      errors: [{ code: 'NOT_FOUND', message: 'missing' }],
    }
    expect(ok.isSuccess).toBe(true)
    expect(fail.isSuccess).toBe(false)
  })
})
