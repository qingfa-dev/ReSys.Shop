import { describe, it, expect } from 'vitest'
import { booleanStatusMap } from '../status'

describe('booleanStatusMap', () => {
  it('has true/Active entry', () => {
    expect(booleanStatusMap['true']).toBeDefined()
    expect(booleanStatusMap['true'].label).toBe('Active')
  })

  it('has false/Inactive entry', () => {
    expect(booleanStatusMap['false']).toBeDefined()
    expect(booleanStatusMap['false'].label).toBe('Inactive')
  })
})
