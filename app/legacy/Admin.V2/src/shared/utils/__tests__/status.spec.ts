import { describe, it, expect } from 'vitest'
import { booleanStatusMap } from '../status'

describe('booleanStatusMap', () => {
  it('has true/Active entry', () => {
    const entry = booleanStatusMap['true']
    expect(entry).toBeDefined()
    expect(entry!.label).toBe('Active')
  })

  it('has false/Inactive entry', () => {
    const entry = booleanStatusMap['false']
    expect(entry).toBeDefined()
    expect(entry!.label).toBe('Inactive')
  })
})
