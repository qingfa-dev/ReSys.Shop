import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createRouter, createMemoryHistory } from 'vue-router'
import type { Router } from 'vue-router'

const { mockStore } = vi.hoisted(() => {
  const store = {
    mockInit: vi.fn<() => Promise<void>>(),
    authenticated: false,
    init() {
      return store.mockInit()
    },
    get isAuthenticated() {
      return store.authenticated
    },
  }
  return { mockStore: store }
})

vi.mock('@/features/auth/stores/authStore', () => ({
  useAuthStore: vi.fn<() => typeof mockStore>(() => mockStore),
}))

async function makeRouter(authenticated: boolean): Promise<Router> {
  // Isolate module-level `isInitialized` so each setup begins uninitialized.
  vi.resetModules()
  mockStore.authenticated = authenticated
  mockStore.mockInit.mockReset()
  mockStore.mockInit.mockResolvedValue(undefined)
  const { setupGuards } = await import('../guards')

  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/login', name: 'login', component: { template: '<div />' } },
      { path: '/', name: 'dashboard', component: { template: '<div />' }, meta: { requiresAuth: true } },
      { path: '/catalog/products', name: 'catalog-products', component: { template: '<div />' }, meta: { requiresAuth: true } },
      { path: '/settings', name: 'settings', component: { template: '<div />' }, meta: { guestOnly: true } },
      { path: '/public', name: 'public', component: { template: '<div />' } },
    ],
  })
  setupGuards(router)
  return router
}

describe('setupGuards', () => {
  beforeEach(() => {
    mockStore.authenticated = false
    mockStore.mockInit.mockReset()
    mockStore.mockInit.mockResolvedValue(undefined)
  })

  it('redirects an unauthenticated user away from a requiresAuth route to login', async () => {
    const router = await makeRouter(false)
    await router.push('/catalog/products')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('login')
    expect(router.currentRoute.value.query.redirect).toBe('/catalog/products')
  })

  it('allows an authenticated user onto a requiresAuth route', async () => {
    const router = await makeRouter(true)
    await router.push('/catalog/products')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('catalog-products')
  })

  it('redirects an authenticated user off a guestOnly route to dashboard', async () => {
    const router = await makeRouter(true)
    await router.push('/settings')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('dashboard')
  })

  it('allows an unauthenticated user onto a guestOnly route', async () => {
    const router = await makeRouter(false)
    await router.push('/settings')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('settings')
  })

  it('calls store.init exactly once across navigations', async () => {
    const router = await makeRouter(false)
    await router.push('/public')
    await router.isReady()

    mockStore.authenticated = true
    await router.push('/catalog/products')
    await router.isReady()

    expect(mockStore.mockInit).toHaveBeenCalledTimes(1)
  })
})