import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import LoginView from '../LoginView.vue'
import { useAuthStore } from '../../stores/authStore'

// Router: Memory-history router with the login target and redirect destinations.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/login', component: LoginView },
      { path: '/register', component: { template: '<div />' } },
      { path: '/forgot-password', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + stubbed pinia so the submit flow stays client-side.
async function mountView() {
  const router = createTestRouter()
  await router.push('/login')
  await router.isReady()
  const wrapper = mount(LoginView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return { wrapper, router }
}

describe('LoginView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the credential, password and remember-me controls', async () => {
    const { wrapper } = await mountView()

    expect(wrapper.text()).toContain('Email or Username')
    expect(wrapper.text()).toContain('Password')
    expect(wrapper.text()).toContain('Remember me')
    expect(wrapper.text()).toContain('Forgot password?')
    expect(wrapper.text()).toContain('Create account')
    expect(wrapper.findAll('[data-pc-name="inputtext"]')).toHaveLength(1)
    expect(wrapper.findAll('[data-pc-name="pcinputtext"]')).toHaveLength(1)
    expect(wrapper.findAll('[data-pc-name="checkbox"]')).toHaveLength(1)
  })

  it('adds no native interactive elements of its own', async () => {
    const { wrapper } = await mountView()

    // Inputs and the submit control come only from PrimeVue: 2 text inputs + 1 checkbox input.
    expect(wrapper.findAll('input')).toHaveLength(3)
    expect(wrapper.findAll('button')).toHaveLength(1)
  })

  it('shows validation messages and skips the store on invalid submit', async () => {
    const { wrapper } = await mountView()

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    const auth = useAuthStore()
    expect(auth.login).not.toHaveBeenCalled()
    expect(wrapper.findAll('[data-pc-name="message"]').length).toBeGreaterThan(0)
  })

  it('calls authStore.login and redirects home on a valid submit', async () => {
    const { wrapper, router } = await mountView()
    const auth = useAuthStore()
    vi.mocked(auth.login).mockResolvedValue(true)

    await wrapper.find('#credential').setValue('alice@example.com')
    await wrapper.find('#password').setValue('supersecret')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    expect(auth.login).toHaveBeenCalledWith('alice@example.com', 'supersecret')
    expect(wrapper.find('[data-pc-name="message"]').text()).toContain('Signed in successfully')
    expect(router.currentRoute.value.path).toBe('/')
  })
})
