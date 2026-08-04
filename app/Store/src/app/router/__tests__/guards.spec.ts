import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createRouter, createMemoryHistory } from 'vue-router'
import type { Router } from 'vue-router'
import { setupGuards } from '../guards'

// The guard consumes the auth store; mock it so we can drive isAuthenticated
// deterministically without pinia or API calls.
vi.mock('@/features/identity/stores/authStore', () => ({
  useAuthStore: vi.fn<(...args: unknown[]) => unknown>(),
}))

import { useAuthStore } from '@/features/identity/stores/authStore'

const mockUseAuthStore = vi.mocked(useAuthStore)

const Dummy = { template: '<div />' }

function createTestRouter(): Router {
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
  return router
}

function stubStore(isAuthenticated: boolean) {
  mockUseAuthStore.mockReturnValue({
    init: vi.fn<(...args: unknown[]) => unknown>().mockResolvedValue(undefined),
    isAuthenticated,
  } as never)
}

describe('setupGuards', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  // NOTE: guards.ts keeps a module-level `isInitialized` flag, so store.init()
  // runs only on the first navigation in this file. This test must run first to
  // assert that behavior; the remaining tests drive isAuthenticated manually and
  // are order-independent.
  it('calls store.init() on first navigation and does not block public routes', async () => {
    stubStore(false)
    const router = createTestRouter()

    await router.push('/public')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/public')
    const store = mockUseAuthStore.mock.results[0]?.value
    expect(store.init).toHaveBeenCalledTimes(1)
  })

  it('redirects unauthenticated users to login with a redirect query', async () => {
    stubStore(false)
    const router = createTestRouter()

    await router.push('/account/orders')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('login')
    expect(router.currentRoute.value.query.redirect).toBe('/account/orders')
  })

  it('allows authenticated users onto protected routes', async () => {
    stubStore(true)
    const router = createTestRouter()

    await router.push('/account/orders')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/account/orders')
  })

  it('redirects authenticated users away from guest-only routes', async () => {
    stubStore(true)
    const router = createTestRouter()

    await router.push('/login')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/')
  })

  it('allows guests onto guest-only routes', async () => {
    stubStore(false)
    const router = createTestRouter()

    await router.push('/login')
    await router.isReady()

    expect(router.currentRoute.value.path).toBe('/login')
  })
})
