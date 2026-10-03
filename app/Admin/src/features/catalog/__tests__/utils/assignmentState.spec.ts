import { describe, it, expect } from 'vitest'
import { makeEmptyAssignments } from '../../utils/assignmentState'

describe('makeEmptyAssignments', () => {
  it('returns empty unassigned and assigned lists', () => {
    const { unassigned, assigned } = makeEmptyAssignments()
    expect(unassigned).toEqual([])
    expect(assigned).toEqual([])
  })
})
