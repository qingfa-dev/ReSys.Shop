import { describe, it, expect, beforeEach, vi } from 'vitest'

const eventListenerMock = vi.fn<(event: string, callback?: unknown) => void>()
const dispatchEventMock = vi.fn<(event: Event) => boolean>()

const matchMediaMock = vi
  .fn<(query: string) => MediaQueryList>()
  .mockImplementation(
    (query: string) =>
      ({
        matches: false,
        media: query,
        onchange: null,
        addEventListener: eventListenerMock,
        removeEventListener: eventListenerMock,
        addListener: eventListenerMock,
        removeListener: eventListenerMock,
        dispatchEvent: dispatchEventMock,
      }) as unknown as MediaQueryList,
  )

describe('useTheme', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', matchMediaMock)
    vi.resetModules()
    localStorage.clear()
    document.documentElement.classList.remove('app-dark')
    vi.restoreAllMocks()
  })

  it('isDark returns false in light mode', async () => {
    localStorage.setItem('theme-preference', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, mode } = useTheme()
    expect(mode.value).toBe('light')
    expect(isDark.value).toBe(false)
  })

  it('isDark returns true in dark mode', async () => {
    localStorage.setItem('theme-preference', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark } = useTheme()
    expect(isDark.value).toBe(true)
  })

  it('applies app-dark class in dark mode', async () => {
    localStorage.setItem('theme-preference', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    useTheme()
    expect(document.documentElement.classList.contains('app-dark')).toBe(true)
  })

  it('removes app-dark class in light mode', async () => {
    document.documentElement.classList.add('app-dark')
    localStorage.setItem('theme-preference', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    useTheme()
    expect(document.documentElement.classList.contains('app-dark')).toBe(false)
  })

  it('toggle cycles light -> dark -> system -> light', async () => {
    localStorage.setItem('theme-preference', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { mode, toggle } = useTheme()
    expect(mode.value).toBe('light')
    toggle()
    expect(mode.value).toBe('dark')
    toggle()
    expect(mode.value).toBe('system')
    toggle()
    expect(mode.value).toBe('light')
  })

  it('setMode persists to localStorage', async () => {
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { setMode } = useTheme()
    setMode('dark')
    expect(localStorage.getItem('theme-preference')).toBe('dark')
  })

  it('defaults to system when no stored preference', async () => {
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { mode } = useTheme()
    expect(mode.value).toBe('system')
  })
})