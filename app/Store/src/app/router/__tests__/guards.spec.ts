import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { Mock } from 'vitest'
import { createRouter, createMemoryHistory } from 'vue-router'
import type { Router } from 'vue-router'

// The guard consumes the auth store; mock it so we can drive isAuthenticated
// deterministically without pinia or API calls.
vi.mock('@/features/identity/stores/authStore', () => ({
  useAuthStore: vi.fn<(...args: unknown[]) => unknown>(),
}))

const Dummy = { template: '<div />' }

// guards.ts keeps a module-level `isInitialized` flag, so store.init() runs only
// on the first navigation per module instance. We dynamically import guards (and
// the mocked auth store) after `vi.resetModules()` so every test gets a fresh
// module state — this keeps the init-once test order-independent (the suite can
// be shuffled without breaking it).
async function createHarness(): Promise<{ router: Router; useAuthStore: Mock }> {
  const { useAuthStore } = await import('@/features/identity/stores/authStore')
  const { setupGuards } = await import('../guards')

  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: Dummy },
      { path: '/public', component: Dummy },
      { path: '/login', name: 'login', component: Dummy, meta: { guestOnly: true } },
      { path: '/account/orders', name: 'orders', component: Dummy, meta: { requiresAuth: true } },
    ],
  })
  setupGuards(router)

  return { router, useAuthStore: useAuthStore as unknown as Mock }
}

function stubStore(useAuthStore: Mock, isAuthenticated: boolean) {
  useAuthStore.mockReturnValue({
    init: vi.fn<(...args: unknown[]) => unknown>().mockResolvedValue(undefined),
    isAuthenticated,
  } as never)
}

describe('setupGuards', () => {
  beforeEach(() => {
    vi.resetModules()
  })

  it('calls store.init() on first navigation and does not block public routes', async () => {
    const { router, useAuthStore } = await createHarness()
    stubStore(useAuthStore, false)

    await router.push('/public')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/public')
    const store = useAuthStore.mock.results[0]?.value
    expect(store.init).toHaveBeenCalledTimes(1)
  })

  it('redirects unauthenticated users to login with a redirect query', async () => {
    const { router, useAuthStore } = await createHarness()
    stubStore(useAuthStore, false)

    await router.push('/account/orders')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('login')
    expect(router.currentRoute.value.query.redirect).toBe('/account/orders')
  })

  it('allows authenticated users onto protected routes', async () => {
    const { router, useAuthStore } = await createHarness()
    stubStore(useAuthStore, true)

    await router.push('/account/orders')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/account/orders')
  })

  it('redirects authenticated users away from guest-only routes', async () => {
    const { router, useAuthStore } = await createHarness()
    stubStore(useAuthStore, true)

    await router.push('/login')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/')
  })

  it('allows guests onto guest-only routes', async () => {
    const { router, useAuthStore } = await createHarness()
    stubStore(useAuthStore, false)

    await router.push('/login')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/login')
  })
})
