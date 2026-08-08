import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import AppHeader from '../AppHeader.vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useSearch } from '@/features/catalog/composables/useSearch'
import { useTheme } from '@/shared/composables/useTheme'

vi.mock('@/features/identity/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('@/features/ordering/stores/cartStore', () => ({
  useCartStore: vi.fn(),
}))

vi.mock('@/features/catalog/composables/useSearch', () => ({
  useSearch: vi.fn(),
}))

vi.mock('@/shared/composables/useTheme', () => ({
  useTheme: vi.fn(),
}))

vi.mock('@/features/ordering/components/CartDrawer.vue', () => ({
  default: { template: '<div />', props: ['visible'] },
}))

vi.mock('../MobileNav.vue', () => ({
  default: { template: '<div />', props: ['open'] },
}))

vi.mock('@/app/components/ThemeToggle.vue', () => ({
  default: { template: '<div />' },
}))

const stubs = {
  'router-link': { template: '<a><slot /></a>' },
  Button: { template: '<button><slot /><template v-if="label">{{ label }}</template></button>', props: ['icon', 'label', 'text', 'rounded', 'size', 'as', 'to', 'severity'] },
  Tag: { template: '<span>{{ value }}</span>', props: ['value', 'severity'] },
  Teleport: { template: '<slot />' },
}

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/login', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
    ],
  })
}

describe('AppHeader', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useAuthStore).mockReturnValue({
      isAuthenticated: false,
      user: null,
      logout: vi.fn(),
    } as never)
    vi.mocked(useCartStore).mockReturnValue({
      itemCount: 0,
    } as never)
    vi.mocked(useSearch).mockReturnValue({
      open: vi.fn(),
    } as never)
    vi.mocked(useTheme).mockReturnValue({
      isDark: { value: false },
      toggle: vi.fn(),
    } as never)
  })

  it('shows Sign In when logged out', async () => {
    vi.mocked(useAuthStore).mockReturnValue({
      isAuthenticated: false,
      user: null,
      logout: vi.fn(),
    } as never)

    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(AppHeader, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Sign In')
  })

  it('does not show Sign In when logged in', async () => {
    vi.mocked(useAuthStore).mockReturnValue({
      isAuthenticated: true,
      user: { userName: 'Test User' },
      logout: vi.fn(),
    } as never)
    vi.mocked(useCartStore).mockReturnValue({
      itemCount: 2,
    } as never)

    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(AppHeader, {
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).not.toContain('Sign In')
  })
})
