import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import ThemeToggle from '../ThemeToggle.vue'

const mockToggle = vi.fn<() => void>()
const mockIsDark = ref(false)

vi.mock('@/shared/composables/useTheme', () => ({
  useTheme: () => ({
    isDark: mockIsDark,
    toggle: mockToggle,
    init: vi.fn<() => void>(),
  }),
}))

const stubs = {
  Button: {
    template: '<button v-bind="$attrs"><slot /></button>',
  },
}

describe('ThemeToggle', () => {
  beforeEach(() => {
    mockToggle.mockClear()
    mockIsDark.value = false
  })

  it('renders sun icon in dark mode', async () => {
    mockIsDark.value = true
    const wrapper = mount(ThemeToggle, { global: { stubs } })
    expect(wrapper.find('button').attributes('aria-label')).toBe('Switch to light mode')
  })

  it('renders moon icon in light mode', async () => {
    mockIsDark.value = false
    const wrapper = mount(ThemeToggle, { global: { stubs } })
    expect(wrapper.find('button').attributes('aria-label')).toBe('Switch to dark mode')
  })

  it('calls toggle on click', async () => {
    const wrapper = mount(ThemeToggle, { global: { stubs } })
    await wrapper.find('button').trigger('click')
    expect(mockToggle).toHaveBeenCalledTimes(1)
  })
})
