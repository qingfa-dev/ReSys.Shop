import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import DefaultLayout from '../DefaultLayout.vue'
import AuthLayout from '../AuthLayout.vue'
import AccountLayout from '../AccountLayout.vue'

vi.mock('@/features/identity/stores/authStore', () => ({
  useAuthStore: vi.fn<() => unknown>().mockReturnValue({ isAuthenticated: false, user: null }),
}))

vi.mock('@/app/components/layout/AppHeader.vue', () => ({
  default: { template: '<header data-testid="app-header">Header</header>' },
}))

vi.mock('@/app/components/layout/AppFooter.vue', () => ({
  default: { template: '<footer data-testid="app-footer">Footer</footer>' },
}))

const DummyView = { template: '<div class="child-content" />' }

function createRouter_(path = '/') {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path, component: DummyView },
      { path: '/account/orders', component: DummyView },
      { path: '/account/addresses', component: DummyView },
      { path: '/account/profile', component: DummyView },
      { path: '/account/sessions', component: DummyView },
      { path: '/account/wishlists', component: DummyView },
      { path: '/account/notifications', component: DummyView },
      { path: '/account/change-password', component: DummyView },
      { path: '/account/preferences', component: DummyView },
    ],
  })
}

describe('DefaultLayout', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders header and footer', async () => {
    const router = createRouter_()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(DefaultLayout, {
      global: { plugins: [router], stubs: { 'router-view': DummyView } },
    })

    expect(wrapper.find('[data-testid="app-header"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="app-footer"]').exists()).toBe(true)
  })
})

describe('AuthLayout', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders centered card with ReSys.Shop branding', async () => {
    const router = createRouter_()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(AuthLayout, {
      global: {
        plugins: [router],
        stubs: {
          'router-link': { template: '<a><slot /></a>' },
          'router-view': DummyView,
        },
      },
    })

    expect(wrapper.text()).toContain('ReSys.Shop')
    expect(wrapper.find('.max-w-md').exists()).toBe(true)
  })
})

describe('AccountLayout', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders sidebar with all 8 nav links', async () => {
    const router = createRouter_()
    await router.push('/account/orders')
    await router.isReady()

    const wrapper = mount(AccountLayout, {
      global: {
        plugins: [router],
        stubs: {
          'router-link': { template: '<a><slot /></a>' },
          'router-view': DummyView,
        },
      },
    })

    const expectedLabels = [
      'Orders', 'Addresses', 'Profile', 'Sessions',
      'Wishlists', 'Notifications', 'Change Password', 'Preferences',
    ]

    for (const label of expectedLabels) {
      expect(wrapper.text()).toContain(label)
    }
  })
})
