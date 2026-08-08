import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ThemeToggle from '../ThemeToggle.vue'
import { useTheme } from '@/shared/composables/useTheme'

// Polyfill: PrimeVue internals call matchMedia on mount; jsdom does not provide it.
function createMatchMediaStub(query: string) {
  return {
    matches: false,
    media: query,
    onchange: null,
    addEventListener: vi.fn<() => void>(),
    removeEventListener: vi.fn<() => void>(),
    addListener: vi.fn<() => void>(),
    removeListener: vi.fn<() => void>(),
    dispatchEvent: vi.fn<() => void>(),
  }
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
})

// Mount: PrimeVue + testing pinia; useTheme stays REAL — the toggle must drive the
// shared module-level singleton so every consumer observes the same state.
function mountToggle() {
  return mount(ThemeToggle, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true })],
    },
  })
}

// Reset: Restore the singleton to a known light state before each test.
function resetTheme(): void {
  localStorage.clear()
  document.documentElement.classList.remove('app-dark')
  useTheme().init()
}

describe('ThemeToggle', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    resetTheme()
  })

  it('renders a ToggleSwitch reflecting the shared dark-mode state', async () => {
    localStorage.setItem('resys_theme', 'dark')
    useTheme().init()
    const wrapper = mountToggle()
    await wrapper.vm.$nextTick()

    const switchInput = wrapper.find('input[type="checkbox"][role="switch"]')
    expect(switchInput.exists()).toBe(true)
    expect((switchInput.element as HTMLInputElement).checked).toBe(true)
    expect(switchInput.attributes('aria-checked')).toBe('true')
  })

  it('toggling flips the shared state and the app-dark document class', async () => {
    const wrapper = mountToggle()
    const { isDark } = useTheme()
    expect(isDark.value).toBe(false)
    expect(document.documentElement.classList.contains('app-dark')).toBe(false)

    await wrapper.find('input[type="checkbox"][role="switch"]').setValue(true)

    // The composable's watchEffect syncs the document class with isDark.
    expect(isDark.value).toBe(true)
    expect(document.documentElement.classList.contains('app-dark')).toBe(true)
    expect(localStorage.getItem('resys_theme')).toBe('dark')

    await wrapper.find('input[type="checkbox"][role="switch"]').setValue(false)

    expect(isDark.value).toBe(false)
    expect(document.documentElement.classList.contains('app-dark')).toBe(false)
    expect(localStorage.getItem('resys_theme')).toBe('light')
  })

  it('adds no native interactive elements of its own', () => {
    const wrapper = mountToggle()

    expect(wrapper.find('button').exists()).toBe(false)
    expect(wrapper.find('label').exists()).toBe(false)
    expect(wrapper.find('select').exists()).toBe(false)
    expect(wrapper.find('textarea').exists()).toBe(false)
  })
})
