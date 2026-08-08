import { describe, it, expect, beforeEach, vi } from 'vitest'

describe('useTheme', () => {
  beforeEach(() => {
    vi.resetModules()
    localStorage.clear()
    document.documentElement.classList.remove('app-dark')
    vi.restoreAllMocks()
  })

  it('isDark returns false in light mode', async () => {
    localStorage.setItem('resys_theme', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, init } = useTheme()
    init()
    expect(isDark.value).toBe(false)
  })

  it('isDark returns true in dark mode', async () => {
    localStorage.setItem('resys_theme', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, init } = useTheme()
    init()
    expect(isDark.value).toBe(true)
  })

  it('applies dark class in dark mode', async () => {
    localStorage.setItem('resys_theme', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { init } = useTheme()
    init()
    expect(document.documentElement.classList.contains('app-dark')).toBe(true)
  })

  it('removes dark class in light mode', async () => {
    document.documentElement.classList.add('app-dark')
    localStorage.setItem('resys_theme', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { init } = useTheme()
    init()
    expect(document.documentElement.classList.contains('app-dark')).toBe(false)
  })

  it('toggle cycles light -> dark -> light', async () => {
    localStorage.setItem('resys_theme', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, toggle, init } = useTheme()
    init()
    expect(isDark.value).toBe(false)
    toggle()
    expect(isDark.value).toBe(true)
    expect(localStorage.getItem('resys_theme')).toBe('dark')
    toggle()
    expect(isDark.value).toBe(false)
    expect(localStorage.getItem('resys_theme')).toBe('light')
  })

  it('init reads from localStorage', async () => {
    localStorage.setItem('resys_theme', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, init } = useTheme()
    init()
    expect(isDark.value).toBe(true)
  })

  it('init defaults to system preference when no stored value', async () => {
    const matchMediaSpy = vi.fn<() => { matches: boolean }>().mockReturnValue({ matches: true })
    vi.stubGlobal('matchMedia', matchMediaSpy)
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, init } = useTheme()
    init()
    expect(isDark.value).toBe(true)
  })
})
