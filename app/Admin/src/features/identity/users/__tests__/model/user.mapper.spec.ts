import { describe, it, expect } from 'vitest'
import { mapUser, mapUserListItem } from '../../model/user.mapper'
import type { User } from '../../model/user.types'

const user: User = {
  id: 'u-1' as never,
  email: 'a@b.co',
  displayName: 'Alice',
  status: 'active',
  roles: ['admin'],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-02T00:00:00Z',
}

describe('user.mapper', () => {
  it('mapUser returns the same shape', () => {
    expect(mapUser(user)).toEqual(user)
  })
  it('mapUserListItem reduces roles to count', () => {
    expect(mapUserListItem(user)).toEqual({
      id: 'u-1',
      email: 'a@b.co',
      displayName: 'Alice',
      status: 'active',
      roleCount: 1,
    })
  })
})
