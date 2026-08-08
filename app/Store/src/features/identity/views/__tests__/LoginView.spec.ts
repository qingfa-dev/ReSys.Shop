import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import LoginView from '../LoginView.vue'

vi.mock('@/shared/composables/usePageTitle', () => ({ usePageTitle: vi.fn() }))
vi.mock('@/shared/composables/useNotify', () => ({
  useNotify: () => ({ success: vi.fn(), error: vi.fn() }),
}))
vi.mock('../../stores/authStore', () => ({
  useAuthStore: () => ({
    login: vi.fn(),
    loginWithGoogle: vi.fn(),
    error: null,
  }),
}))

const stubs = {
  Breadcrumb: { template: '<div><slot /></div>' },
  Card: { template: '<div class="card"><slot name="content" /></div>' },
  InputText: { template: '<input />', props: ['modelValue'] },
  Password: { template: '<input type="password" />', props: ['modelValue'] },
  Button: { template: '<button>{{ label }}</button>', props: ['label'] },
}

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/login', component: { template: '<div />' } },
  ],
})

describe('LoginView', () => {
  it('renders email and password fields', async () => {
    router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginView, {
      global: { plugins: [router], stubs },
    })
    expect(wrapper.html()).toContain('Sign In')
    expect(wrapper.html()).toContain('Forgot password')
  })

  it('renders Google login button', async () => {
    router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginView, {
      global: { plugins: [router], stubs },
    })
    expect(wrapper.html()).toContain('Google')
  })

  it('renders register link', async () => {
    router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginView, {
      global: { plugins: [router], stubs },
    })
    expect(wrapper.html()).toContain('Create one')
  })
})
