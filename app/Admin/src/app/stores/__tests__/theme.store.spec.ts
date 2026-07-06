import { describe, it, expect, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { nextTick } from 'vue'
import { useThemeStore } from '../theme.store'

describe('theme.store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    document.documentElement.classList.remove('p-dark')
  })

  it('toggles dark mode and updates DOM', async () => {
    const s = useThemeStore()
    expect(s.isDark).toBe(false)
    s.toggle()
    expect(s.isDark).toBe(true)
    await nextTick()
    expect(document.documentElement.classList.contains('p-dark')).toBe(true)
    s.toggle()
    expect(s.isDark).toBe(false)
  })
})

