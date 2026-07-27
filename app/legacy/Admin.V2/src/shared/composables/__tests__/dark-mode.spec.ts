import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { nextTick } from 'vue'
import { useDarkMode } from '../useDarkMode'

describe('useDarkMode', () => {
  beforeEach(() => { localStorage.clear() })
  afterEach(() => { localStorage.clear() })

  it('isDark defaults to false', () => {
    const { isDark } = useDarkMode()
    expect(isDark.value).toBe(false)
  })

  it('toggle flips isDark', () => {
    const { isDark, toggle } = useDarkMode()
    toggle()
    expect(isDark.value).toBe(true)
    toggle()
    expect(isDark.value).toBe(false)
  })

  it('enable sets isDark true and persists to localStorage', async () => {
    const { isDark, enable } = useDarkMode()
    enable()
    await nextTick()
    expect(isDark.value).toBe(true)
    expect(localStorage.getItem('resys-admin-dark-mode')).toBe('true')
  })

  it('disable sets isDark false', async () => {
    const { isDark, enable, disable } = useDarkMode()
    enable()
    await nextTick()
    disable()
    await nextTick()
    expect(isDark.value).toBe(false)
  })
})
