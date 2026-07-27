import { describe, it, expect } from 'vitest'
import { STORAGE_KEYS } from './storage'

describe('STORAGE_KEYS', () => {
  it('contains all expected keys', () => {
    expect(STORAGE_KEYS).toEqual({
      ACCESS_TOKEN: 'accessToken',
      REFRESH_TOKEN: 'refreshToken',
      USER: 'currentUser',
      LAYOUT: 'resys-admin-layout',
      THEME: 'resys-admin-theme',
      LOCALE: 'resys-admin-locale',
    })
  })
})
