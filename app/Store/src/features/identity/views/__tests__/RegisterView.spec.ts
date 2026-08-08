import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import RegisterView from '../RegisterView.vue'

vi.mock('@/shared/composables/usePageTitle', () => ({ usePageTitle: vi.fn() }))
vi.mock('@/shared/composables/useNotify', () => ({
  useNotify: () => ({ success: vi.fn(), error: vi.fn() }),
}))
vi.mock('../../stores/authStore', () => ({
  useAuthStore: () => ({
    register: vi.fn(),
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
    { path: '/register', component: { template: '<div />' } },
  ],
})

describe('RegisterView', () => {
  it('renders all form fields', async () => {
    router.push('/register')
    await router.isReady()
    const wrapper = mount(RegisterView, {
      global: { plugins: [router], stubs },
    })
    expect(wrapper.html()).toContain('Create Account')
    expect(wrapper.html()).toContain('Full name')
    expect(wrapper.html()).toContain('Email')
  })

  it('renders Google login button', async () => {
    router.push('/register')
    await router.isReady()
    const wrapper = mount(RegisterView, {
      global: { plugins: [router], stubs },
    })
    expect(wrapper.html()).toContain('Google')
  })

  it('renders login link', async () => {
    router.push('/register')
    await router.isReady()
    const wrapper = mount(RegisterView, {
      global: { plugins: [router], stubs },
    })
    expect(wrapper.html()).toContain('Sign In')
  })
})
