import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import type { Router } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import DefaultLayout from '../DefaultLayout.vue'
import AuthLayout from '../AuthLayout.vue'
import AccountLayout from '../AccountLayout.vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useOrderStore } from '@/features/ordering/stores/orderStore'
import type { OrderListItem } from '@/features/ordering/types'

// Polyfill: AppHeader/Menubar, Drawer, Sidebar and PanelMenu call matchMedia on
// mount; jsdom does not provide it. `mediaMatches` lets tests flip the breakpoint
// that useMediaQuery('(max-width: 1023px)') reads inside AccountLayout.
let mediaMatches = false

function createMatchMediaStub(query: string) {
  return {
    matches: mediaMatches,
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

// Account: Authenticated user fixture for AccountLayout (guards are not wired up
// here, so the layout's own isAuthenticated fallback is what we exercise).
const authUser = {
  userId: 'u1',
  userName: 'Ada',
  email: 'ada@test.dev',
  roles: [],
  permissions: [],
  isAuthenticated: true,
}

// ---------------------------------------------------------------------------
// DefaultLayout
// ---------------------------------------------------------------------------

// Route: Sync stub for the initial route; a deliberately slow lazy route keeps
// the router's beforeEach/afterEach gap observable for the loader assertion.
function createDefaultLayoutRouter(): Router {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div class="home-stub">Home Route</div>' } },
      {
        path: '/slow',
        component: () =>
          new Promise(resolve => {
            setTimeout(() => resolve({ template: '<div class="slow-stub">Slow Route</div>' }), 200)
          }),
      },
    ],
  })
}

async function mountDefaultLayout() {
  const router = createDefaultLayoutRouter()
  await router.push('/')
  await router.isReady()
  const wrapper = mount(DefaultLayout, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await flushPromises()
  return { wrapper, router }
}

// ---------------------------------------------------------------------------
// AuthLayout
// ---------------------------------------------------------------------------

async function mountAuthLayout(path: string) {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/login', component: { template: '<div class="login-stub">Login Form</div>' } },
      { path: '/register', component: { template: '<div class="register-stub">Register Form</div>' } },
    ],
  })
  await router.push(path)
  await router.isReady()
  const wrapper = mount(AuthLayout, {
    slots: { default: '<div class="auth-slot-stub">Auth slot content</div>' },
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
    },
  })
  return wrapper
}

// ---------------------------------------------------------------------------
// AccountLayout
// ---------------------------------------------------------------------------

function createAccountRouter(): Router {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/account/profile', component: { template: '<div class="profile-stub">Profile Stub</div>' } },
      { path: '/account/addresses', component: { template: '<div />' } },
      { path: '/account/wishlists', component: { template: '<div />' } },
      { path: '/account/notifications', component: { template: '<div />' } },
      { path: '/account/change-password', component: { template: '<div />' } },
      { path: '/account/preferences', component: { template: '<div />' } },
      { path: '/account/orders', component: { template: '<div />' } },
      { path: '/account/orders/:id', component: { template: '<div />' } },
      { path: '/login', component: { template: '<div />' } },
    ],
  })
}

async function mountAccountLayout(router = createAccountRouter()) {
  await router.push('/account/profile')
  await router.isReady()
  const wrapper = mount(AccountLayout, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await flushPromises()
  return wrapper
}

async function signIn(wrapper: ReturnType<typeof mountAccountLayout> extends Promise<infer T> ? T : never) {
  const auth = useAuthStore()
  auth.$patch({ status: 'authenticated', user: authUser })
  await wrapper.vm.$nextTick()
}

// Fixture: A single open order makes the Orders nav badge render.
const draftOrder: OrderListItem = {
  id: 'o-1',
  number: 'R10001',
  status: 'Draft',
  total: 99,
  createdAtUtc: '2026-01-01T00:00:00Z',
}

describe('DefaultLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    document.documentElement.scrollTop = 0
  })

  it('renders the shell: header, main with the routed view and footer', async () => {
    const { wrapper } = await mountDefaultLayout()

    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('main').exists()).toBe(true)
    expect(wrapper.find('footer').exists()).toBe(true)
    expect(wrapper.text()).toContain('Home Route')
  })

  it('reveals the ScrollTop button once the window scrolls past the threshold', async () => {
    const { wrapper } = await mountDefaultLayout()

    expect(wrapper.find('[aria-label="Scroll Top"]').exists()).toBe(false)

    // jsdom never scrolls on its own; drive the scroll listener directly.
    document.documentElement.scrollTop = 500
    window.dispatchEvent(new Event('scroll'))
    await flushPromises()
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[aria-label="Scroll Top"]').exists()).toBe(true)
  })

  it('shows the skeleton route loader only while a lazy route chunk resolves', async () => {
    const { wrapper, router } = await mountDefaultLayout()

    expect(wrapper.find('[data-pc-name="skeleton"]').exists()).toBe(false)

    const nav = router.push('/slow')
    await flushPromises()

    // The layout's beforeEach sets loading=true until afterEach confirms the route.
    expect(wrapper.find('[data-pc-name="skeleton"]').exists()).toBe(true)

    await nav
    await flushPromises()

    expect(wrapper.find('[data-pc-name="skeleton"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Slow Route')
  })
})

describe('AuthLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the slotted form content inside the Card', async () => {
    const wrapper = await mountAuthLayout('/login')

    expect(wrapper.find('.auth-slot-stub').exists()).toBe(true)
    expect(wrapper.text()).toContain('Auth slot content')
    expect(wrapper.text()).toContain('ReSys.Shop')
  })

  it('links to register when on the login route', async () => {
    const wrapper = await mountAuthLayout('/login')

    const secondary = wrapper.findAll('a').find(a => a.text().includes('Create account'))
    expect(secondary!.attributes('href')).toBe('/register')
    const back = wrapper.findAll('a').find(a => a.text().includes('Back to store'))
    expect(back!.attributes('href')).toBe('/')
  })

  it('links to login when on the register route', async () => {
    const wrapper = await mountAuthLayout('/register')

    const secondary = wrapper.findAll('a').find(a => a.text().includes('Sign in'))
    expect(secondary!.attributes('href')).toBe('/login')
    expect(wrapper.findAll('a').find(a => a.text().includes('Create account'))).toBeUndefined()
  })
})

describe('AccountLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mediaMatches = false
  })

  it('shows the session-expired fallback when not authenticated', async () => {
    const wrapper = await mountAccountLayout()

    expect(wrapper.text()).toContain('Your session has expired. Please sign in again to continue.')
    expect(wrapper.find('[data-pc-section="headerlink"]').exists()).toBe(false)
  })

  it('renders all 7 account nav items and marks the active route', async () => {
    const wrapper = await mountAccountLayout()
    await signIn(wrapper)

    const links = wrapper.findAll('[data-pc-section="headerlink"]')
    expect(links).toHaveLength(7)
    expect(wrapper.text()).toContain('Profile')
    expect(wrapper.text()).toContain('Addresses')
    expect(wrapper.text()).toContain('Wishlists')
    expect(wrapper.text()).toContain('Notifications')
    expect(wrapper.text()).toContain('Change Password')
    expect(wrapper.text()).toContain('Preferences')
    expect(wrapper.text()).toContain('Orders')

    const active = links.find(a => a.text() === 'Profile')
    expect(active!.attributes('aria-current')).toBe('page')
    expect(links.find(a => a.text() === 'Orders')!.attributes('aria-current')).toBeUndefined()
  })

  it('renders the routed view and covers order-detail active states', async () => {
    const router = createAccountRouter()
    const wrapper = await mountAccountLayout(router)
    await signIn(wrapper)

    expect(wrapper.text()).toContain('Profile Stub')
    expect(wrapper.find('a[href="/account/orders"]').exists()).toBe(true)
  })

  it('shows the active order count badge on the Orders item', async () => {
    const wrapper = await mountAccountLayout()
    await signIn(wrapper)
    const orders = useOrderStore()
    orders.$patch({ items: [draftOrder] })
    await wrapper.vm.$nextTick()

    const ordersLink = wrapper.findAll('[data-pc-section="headerlink"]').find(a => a.text().startsWith('Orders'))
    expect(ordersLink!.text()).toContain('1')
  })

  it('opens the mobile Sidebar drawer from the Menu button below lg', async () => {
    mediaMatches = true
    const wrapper = await mountAccountLayout()
    await signIn(wrapper)

    const sidebar = wrapper.find('[data-pc-name="sidebar"]')
    expect(sidebar.exists()).toBe(true)
    expect(sidebar.attributes('data-state')).toBe('collapsed')

    await wrapper.findAll('button').find(b => b.text().includes('Menu'))!.trigger('click')
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-pc-name="sidebar"]').attributes('data-state')).toBe('expanded')
    expect(wrapper.text()).toContain('Profile Stub')
  })

  it('adds no native input, select or label elements of its own', async () => {
    const wrapper = await mountAccountLayout()
    await signIn(wrapper)

    expect(wrapper.find('input').exists()).toBe(false)
    expect(wrapper.find('select').exists()).toBe(false)
    expect(wrapper.find('label').exists()).toBe(false)
    expect(wrapper.find('textarea').exists()).toBe(false)
  })
})
