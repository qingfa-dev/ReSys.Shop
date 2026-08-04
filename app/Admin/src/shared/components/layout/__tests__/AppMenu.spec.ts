import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import AppMenu from '../AppMenu.vue'
import * as authApi from '@/features/auth/services/authApi'

let toastMock = { add: vi.fn<(...args: unknown[]) => unknown>() }

vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn<(...args: unknown[]) => unknown>(() => toastMock),
}))

vi.mock('@/features/auth/services/authApi', () => ({
  logout: vi.fn<(...args: unknown[]) => unknown>(() => Promise.resolve({ isSuccess: true, value: undefined })),
}))

vi.mock('@/features/dashboard/routes', () => ({ dashboardMenuItems: [{ label: 'Dashboard', icon: 'pi pi-home', route: '/' }] }))
vi.mock('@/features/catalog/routes', () => ({ catalogMenuItems: [] }))
vi.mock('@/features/identity/routes', () => ({ identityMenuItems: [] }))
vi.mock('@/features/inventory/routes', () => ({ inventoryMenuItems: [] }))
vi.mock('@/features/location/routes', () => ({ locationMenuItems: [] }))
vi.mock('@/features/ordering/routes', () => ({ orderingMenuItems: [] }))
vi.mock('@/features/payment/routes', () => ({ paymentMenuItems: [] }))
vi.mock('@/features/profile/routes', () => ({ profileMenuItems: [] }))
vi.mock('@/features/shipping/routes', () => ({ shippingMenuItems: [] }))

function createWrapper(isLoggingOut = false) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [{ path: '/auth/login', name: 'login', component: { template: '<div>login</div>' } }],
  })

  return { wrapper: mount(AppMenu, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
          initialState: {
            auth: {
              user: { userId: 'u1', userName: 'User One', email: 'u1@test.com', roles: [], permissions: [], isAuthenticated: true },
              status: 'authenticated',
              isLoggingOut,
            },
          },
        }),
        router,
        PrimeVue,
        ToastService,
      ],

    },
  }), router }
}

beforeEach(() => {
  vi.clearAllMocks()
  toastMock = { add: vi.fn<(...args: unknown[]) => unknown>() }
})

describe('AppMenu', () => {
  it('renders logout menu item with sign-out icon', () => {
    const { wrapper } = createWrapper()
    const logoutItem = wrapper.find('.logout-item')
    expect(logoutItem.exists()).toBe(true)
    expect(logoutItem.text()).toContain('Logout')
  })

  it('calls authStore.logout and shows toast on logout click', async () => {
    const { wrapper } = createWrapper()
    await wrapper.find('.logout-item').trigger('click')

    expect(authApi.logout).toHaveBeenCalled()
    await new Promise(resolve => setTimeout(resolve, 0))
    expect(toastMock.add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', summary: 'Logged out' }),
    )
  })

  it('redirects to login after logout', async () => {
    const { wrapper, router } = createWrapper()
    await router.isReady()
    await wrapper.find('.logout-item').trigger('click')
    await new Promise(resolve => setTimeout(resolve, 0))
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('applies disabled styling when isLoggingOut is true', () => {
    const { wrapper } = createWrapper(true)
    const logoutItem = wrapper.find('.logout-item')
    expect(logoutItem.classes()).toContain('pointer-events-none')
    expect(logoutItem.classes()).toContain('opacity-50')
  })
})
