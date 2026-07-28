import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import UserMenu from '../UserMenu.vue'
import * as authApi from '@/features/auth/services/authApi'

const mockToastAdd = vi.fn()
vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn(() => ({ add: mockToastAdd })),
}))

vi.mock('@/features/auth/services/authApi', () => ({
  logout: vi.fn(() => Promise.resolve({ isSuccess: true, value: undefined })),
}))

function createWrapper(authOverrides = {}) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [{ path: '/auth/login', name: 'login', component: { template: '<div>login</div>' } }],
  })

  const wrapper = mount(UserMenu, {
    attachTo: document.body,
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
          initialState: {
            auth: {
              user: { userId: 'u1', roles: [], permissions: [], isAuthenticated: true },
              status: 'authenticated',
              isLoggingOut: false,
              ...authOverrides,
            },
          },
        }),
        router,
        PrimeVue,
        ToastService,
      ],
    },
  })

  return { wrapper, router }
}

function flush() {
  return new Promise(resolve => setTimeout(resolve, 50))
}

beforeEach(() => {
  vi.clearAllMocks()
  document.body.innerHTML = ''
})

describe('UserMenu', () => {
  it('renders avatar and user ID when authenticated', () => {
    const { wrapper } = createWrapper()
    expect(wrapper.text()).toContain('u1')
    expect(wrapper.find('.p-avatar').exists()).toBe(true)
  })

  it('does not render when not authenticated', () => {
    const { wrapper } = createWrapper({
      user: null,
      status: 'idle',
    })
    expect(wrapper.find('.p-avatar').exists()).toBe(false)
  })

  it('opens popover on avatar click', async () => {
    const { wrapper } = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.vm.$nextTick()
    expect(document.body.querySelector('.p-popover')).toBeTruthy()
  })

  it('shows logout button in popover', async () => {
    const { wrapper } = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.vm.$nextTick()
    const logoutBtn = document.body.querySelector('button.logout-btn')
    expect(logoutBtn).toBeTruthy()
  })

  it('calls authStore.logout and shows toast on logout click', async () => {
    const { wrapper } = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.vm.$nextTick()

    const logoutBtn = document.body.querySelector('button.logout-btn') as HTMLElement
    logoutBtn?.click()
    await flush()

    expect(authApi.logout).toHaveBeenCalled()
    expect(mockToastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', summary: 'Logged out' }),
    )
  })

  it('disables logout button when isLoggingOut is true', async () => {
    const { wrapper } = createWrapper({ isLoggingOut: true })
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.vm.$nextTick()
    const logoutBtn = document.body.querySelector('button.logout-btn')
    expect(logoutBtn?.getAttribute('disabled')).toBeDefined()
  })

  it('redirects to login after logout', async () => {
    const { router, wrapper } = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.vm.$nextTick()

    const logoutBtn = document.body.querySelector('button.logout-btn') as HTMLElement
    logoutBtn?.click()
    await flush()

    expect(router.currentRoute.value.name).toBe('login')
  })
})
