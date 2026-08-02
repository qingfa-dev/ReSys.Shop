import { describe, it, expect } from 'vitest'
import { statusAction } from '../../utils/productStatus'

describe('statusAction', () => {
  it('returns discontinue for Active', () => {
    expect(statusAction('Active')).toEqual({ kind: 'discontinue' })
  })
  it('returns activate for Draft', () => {
    expect(statusAction('Draft')).toEqual({ kind: 'activate' })
  })
  it('returns activate for Archived', () => {
    expect(statusAction('Archived')).toEqual({ kind: 'activate' })
  })
})
